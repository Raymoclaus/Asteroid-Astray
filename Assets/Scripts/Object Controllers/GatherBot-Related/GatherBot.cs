using CustomDataTypes;
using InventorySystem;
using LineRendererControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GatherBot : Character, IStunnable, ICombat
{
	protected enum AIState
	{
		//exit the hive and find a position to begin scanning
		Spawning,
		//looking for resources to gather
		Scanning,
		//found resources, now trying to harvest
		Gathering,
		//didn't find resources, now exploring a bit to scan a different area
		Exploring,
		//inventory full, returning to hive to store findings
		Storing,
		//scan found an unknown entity, determing threat level
		Suspicious,
		//calling for reinforcements
		Signalling,
		//attacking unknown entity
		Attacking,
		//collecting spoils of a skirmish
		Collecting,
		//Running for backup when solo fight gets too intense
		Escaping,
		//Exploring with no real direction
		Wandering,
		//Dying
		Dying
	}

	[Header("Gather Bot Fields")]
	//references
	protected BotHive hive;
	[SerializeField] private ShakeEffect shakeFX;
	[SerializeField] private Animator anim;
	[SerializeField] private SpriteRenderer sprRend;

	//fields
	[SerializeField] private AIState state;
	[HideInInspector] public int upgradeLevel;
	public int dockID = -1;

	//spawning variables
	private bool activated = false;

	//movement variables
	private Vector2 accel;
	[SerializeField] private float engineStrength = 1f, speedLimit = 2f, deceleration = 2f,
		decelEffectiveness = 0.01f, rotationSpeed = 3f;
	private float rot;
	private Vector2 velocity;
	private float maxSway = 45;
	private float distanceCheck = 10f;
	public Entity targetEntity;

	//scanning variables
	[SerializeField] private ExpandingCircle scanningBeam;
	private float scanTimer, scanDuration = 3f;
	private bool scanStarted;
	private List<Entity> entitiesScanned = new List<Entity>();
	private bool isIdle = true;

	//gathering variables
	private float searchTimer, scanInterval = 0.3f;
	private int drillCount, drillLimit = 3;
	[SerializeField] private int storageCapacity = 10;
	private int itemsCollected;
	private bool waitingForResources;

	//exploring variables
	public Vector2 targetLocation;

	//storing variables
	private float minDistance = 1f;
	private float dockDistance = 1.5f;
	private bool disassembling = false;
	private float rotToDockSpeed = 100f;
	private float moveToDockSpeed = 1f;

	//suspicious variables
	private float suspiciousChaseRange = 8f;
	private float intenseScanDuration = 4f;
	private float intenseScanTimer = 0f;
	private float intenseScanRange = 1.5f;
	private List<ICombat> nearbySuspects = new List<ICombat>();
	[SerializeField] private ParticleSystem intenseScanner;
	[SerializeField] private SpriteRenderer radialMeterPrefab;
	private SpriteRenderer suspicionMeter;
	[SerializeField] private Vector2 meterRelativePosition = Vector2.one * 0.2f;
	protected List<Entity> unsuspiciousEntities = new List<Entity>();

	//combat variables
	[SerializeField] private StraightWeapon straightWeapon;
	private bool beingDrilled;
	private List<ICombat> enemies = new List<ICombat>();
	protected bool IsInCombat { get { return enemies.Count > 0; } }
	protected bool IsSwarmInCombat
	{
		get
		{
			for (int i = 0; i < hive?.childBots.Count; i++)
			{
				if (!hive.childBots[i].IsInCombat) return false;
			}
			return IsInCombat;
		}
	}
	private float chaseRange = 16f;
	[SerializeField] private float outOfRangeCountdown = 8f;
	private float outOfRangeTimer;
	[SerializeField] private float orbitRange = 1.75f;
	[SerializeField] private float orbitSpeed = 0.6f;
	[SerializeField] private float firingRange = 3f;
	[SerializeField] protected int valuableLootThreshold = 1;
	[SerializeField] private float drillToChargeTimer = 1f;
	[SerializeField] private float chargeToExplosionTimer = 3f;
	[SerializeField] private SpriteRenderer chargeSprRend;
	[SerializeField] private float explosionRadius = 3f;
	[SerializeField] private float explosionStrength = 1f;
	private bool stunned = false;
	[SerializeField] private float stunDuration = 2f;
	private float stunTimer = 0f;
	public float drillDamageResistance = 2f;
	[SerializeField] private ExpandingCircle forcePulseWave;
	private bool straightWeaponAttached = false;
	private bool readyToFire = false;
	[SerializeField] private ColorReplacement colorReplacement;
	private RaycastHit2D[] lineOfSight = new RaycastHit2D[2];

	//signalling variables
	private float signalTimer;

	//escaping variables
	private List<Entity> scaryEntities = new List<Entity>();
	private Entity currentlyEscaping;

	//dying variables
	private float dyingTimer = 0f;
	[SerializeField] private float minimumDyingDuration = 3f;
	[SerializeField] private float maximumDyingDuration = 6f;
	[SerializeField] private Sprite dyingSprite;
	private Entity destroyer;
	private float dropModifier;
	[SerializeField] private GameObject burningEffectsObj;
	[SerializeField] private GameObject explosionDeathObj;

	private void Start()
	{
		rot = transform.eulerAngles.z;
		OnSpawn();
	}

	protected virtual void OnSpawn()
	{
		if (hive == null)
		{
			DestroySelf(null, 0f);
		}

		ActivateRenderers(false);
	}

	protected override void Update()
	{
		accel = Vector2.zero;
		if (stunned)
		{
			stunTimer -= Time.deltaTime;
			stunned = stunTimer <= 0f;
		}
		if (beingDrilled || launched || !activated) return;

		switch (state)
		{
			default:
			case AIState.Spawning:
				Spawning();
				break;
			case AIState.Scanning:
				Scanning();
				break;
			case AIState.Gathering:
				Gathering();
				break;
			case AIState.Exploring:
				Exploring();
				break;
			case AIState.Storing:
				Storing();
				break;
			case AIState.Suspicious:
				Suspicious();
				break;
			case AIState.Signalling:
				Signalling();
				break;
			case AIState.Attacking:
				Attacking();
				break;
			case AIState.Collecting:
				Collecting();
				break;
			case AIState.Escaping:
				Escaping();
				break;
			case AIState.Wandering:
				Wandering();
				break;
			case AIState.Dying:
				Dying();
				break;
		}

		ApplyMovementCalculation();
	}

	private void FixedUpdate() => rb.AddForce(accel);
	
	protected virtual void Spawning()
	{
		Transform dock = hive.GetDock(this);
		Vector2 targetPos = dock.position - dock.up * dockDistance;
		if (GoToLocation(targetPos))
		{
			if (rb.velocity.sqrMagnitude < 0.01f)
			{
				SetState(AIState.Exploring);
			}
		}
	}

	protected virtual void Scanning()
	{
		if (rb.velocity.sqrMagnitude < 0.01f)
		{
			if (!scanStarted)
			{
				if (IsInViewRange)
				{
					StartCoroutine(ScanRings());
				}
				scanStarted = true;
				entitiesScanned.Clear();
				new Thread(() =>
				{
					EntityNetwork.IterateEntitiesInRange(
						coords,
						1,
						e =>
						{
							if (e != this)
							{
								entitiesScanned.Add(e);
							}

							return false;
						});
				}).Start();
			}
			else
			{
				scanTimer += Time.deltaTime;
			}

			if (scanTimer >= scanDuration)
			{
				scanTimer = 0f;
				scanStarted = false;
				//look for the first unusual entity in the scan
				for (int i = 0; i < entitiesScanned.Count; i++)
				{
					Entity e = entitiesScanned[i];
					if (IsScary(e))
					{
						hive?.MarkCoordAsEmpty(coords);
						SetState(AIState.Exploring);
						return;
					}
					ICombat threat = e.GetICombat();
					if (threat != null && IsSuspicious(threat))
					{
						nearbySuspects.Add(threat);
					}
				}

				if (nearbySuspects.Count > 0)
				{
					SetState(AIState.Suspicious);
					return;
				}

				//choose state
				if (entitiesScanned.Count >= 10)
				{
					SetState(AIState.Gathering);
				}
				else
				{
					hive?.MarkCoordAsEmpty(coords);
					SetState(AIState.Exploring);
				}
			}
		}
	}

	protected virtual void Gathering()
	{
		if (waitingForResources) return;

		if (TimeController.TimeSinceOpen - searchTimer > scanInterval || targetEntity == null)
		{
			SearchForNearestAsteroid();
		}
		Vector2 targetPos = targetEntity.transform.position;
		if (!targetEntity.gameObject.activeSelf)
		{
			GoToLocation(targetPos, false, 1f, true);
			float distLeft = Vector2.Distance(transform.position, targetPos);
			if (distLeft <= 1f)
			{
				if (targetEntity.TakeDrillDamage(DrillDamageQuery(false),
					drill.transform.position, this, 1f))
				{
					DrillComplete();
				}
			}
		}
		else
		{
			GoToLocation(targetPos, false, 0f, true);
		}
	}

	protected virtual void Exploring()
	{
		if (GoToLocation(targetLocation, true))
		{
			SetState(AIState.Scanning);
		}
	}

	protected virtual void Storing()
	{
		//move towards disassembly point
		Transform dock = hive.GetDock(this);
		if (disassembling)
		{
			rot = Mathf.MoveTowardsAngle(rot, dock.eulerAngles.z, rotToDockSpeed * Time.deltaTime);
			if (rot == dock.eulerAngles.z)
			{
				transform.position = Vector2.MoveTowards(
					transform.position, dock.position, moveToDockSpeed * Time.deltaTime);
				if (Vector2.Distance(transform.position, dock.position) < 0.01f)
				{
					//start disassembly animation
					SetState(AIState.Spawning);
				}
			}
			return;
		}

		//start disassembling process if close enough to hive dock
		Vector2 targetPos = dock.position - dock.up * dockDistance;
		float distanceFromDockEntry = Vector2.Distance(transform.position, targetPos);
		if (distanceFromDockEntry < minDistance)
		{
			disassembling = true;
			return;
		}

		//go back to hive
		GoToLocation(targetPos, true, minDistance, false);
	}

	protected virtual void Suspicious()
	{
		if (nearbySuspects.Count == 0)
		{
			SetState(AIState.Scanning);
			return;
		}
		targetEntity = (Entity)nearbySuspects[0];
		//if target is too far away then return to exploring
		if (targetEntity == null ||
			Vector2.Distance(targetEntity.transform.position, transform.position) > suspiciousChaseRange)
		{
			intenseScanTimer = 0f;
			nearbySuspects.RemoveAt(0);
			suspicionMeter.gameObject.SetActive(false);
			suspicionMeter.material.SetFloat("_ArcAngle", 0);
			return;
		}

		//follow entity
		GoToLocation(targetEntity.transform.position, true, intenseScanRange, true, targetEntity.transform.position);
		if (Vector2.Distance(transform.position, targetEntity.transform.position) <= intenseScanRange)
		{
			//scan entity if close enough
			intenseScanTimer += Time.deltaTime;

			//draw scanner particles
			if (!intenseScanner.isPlaying) intenseScanner.Play();
			float angle = -Vector2.SignedAngle(Vector2.up, targetEntity.transform.position - transform.position);
			intenseScanner.transform.eulerAngles = Vector3.forward * -angle;
			intenseScanner.transform.localScale = Vector3.one *
				Vector2.Distance(transform.position, targetEntity.transform.position);

			//draw suspicion meter
			suspicionMeter.gameObject.SetActive(true);
			suspicionMeter.material.SetFloat("_ArcAngle", intenseScanTimer / intenseScanDuration * 360f);

			//check if scan is complete
			if (intenseScanTimer >= intenseScanDuration)
			{
				intenseScanner.Stop();
				suspicionMeter.gameObject.SetActive(false);
				suspicionMeter.material.SetFloat("_ArcAngle", 0);
				//scan complete
				intenseScanTimer = 0f;
				//assess threat level and make a decision
				AttackViability threatLevel = EvaluateScan(targetEntity.ReturnScan());
				switch (threatLevel)
				{
					case AttackViability.AttackAlone:
						ICombat enemy = targetEntity.GetICombat();
						if (!(bool)enemy?.EngageInCombat(this)) break;
						EngageInCombat(enemy);
						SetState(AIState.Attacking);
						break;
					case AttackViability.SignalForHelp:
						enemy = targetEntity.GetICombat();
						if (!(bool)enemy?.EngageInCombat(this)) break;
						EngageInCombat(enemy);
						SetState(AIState.Signalling);
						break;
					case AttackViability.Escape:
						hive?.MarkCoordAsEmpty(targetEntity.GetCoords());
						scaryEntities.Add(targetEntity);
						SetState(AIState.Exploring);
						break;
					case AttackViability.Ignore:
						unsuspiciousEntities.Add((Entity)nearbySuspects[0]);
						nearbySuspects.RemoveAt(0);
						break;
				}
			}
		}
		else
		{
			intenseScanner.Stop();
		}
		suspicionMeter.transform.position = (Vector2)transform.position + meterRelativePosition;
	}

	protected virtual void Signalling()
	{
		signalTimer -= Time.deltaTime;
		if (signalTimer <= 0f)
		{
			for (int i = 0; i < hive.childBots.Count; i++)
			{
				GatherBot sibling = hive.childBots[i];
				sibling.enemies = enemies;
				if (IsInCombat)
				{
					sibling.SetState(AIState.Escaping);
				}
				else
				{
					sibling.SetState(AIState.Attacking);
				}
				for (int j = 0; j < enemies.Count; j++)
				{
					ICombat enemy = enemies[j];
					enemy.EngageInCombat(sibling);
				}
			}
		}
	}

	protected virtual void Attacking()
	{
		if (enemies.Count == 0)
		{
			SetState(AIState.Collecting);
		}

		Vector2 currentPos = transform.position;

		//if sibling bots are nearby, get them to join the fight
		for (int i = 0; i < hive?.childBots.Count; i++)
		{
			GatherBot sibling = hive.childBots[i];
			if (sibling.state == AIState.Attacking || sibling.state == AIState.Dying) continue;

			if (Vector2.Distance(currentPos, sibling.transform.position)
				< Constants.CHUNK_SIZE)
			{
				float scanAngle = -Vector2.SignedAngle(Vector2.up, (Vector2)sibling.transform.position - currentPos);
				StartCoroutine(ScanRings(scanAngle, 30f, false, 0.3f));
				sibling.enemies = enemies;
				sibling.SetState(AIState.Attacking);
			}
		}

		//check if the target is too far away from this bot and its siblings
		targetEntity = (Entity)enemies[0];
		Vector2 enemyPos = targetEntity.transform.position;
		bool found = SiblingsInRangeOfTarget(enemyPos);

		//if the target is out of range for too long then disengage from combat
		if (IncrementOutOfRangeCounter(found)) return;

		//bots attack by circling its target and firing
		float orbitAngle = Mathf.PI * 2f / (hive?.childBots.Count ?? 1) * dockID + TimeController.TimeSinceOpen * orbitSpeed;
		Vector2 orbitPos = new Vector2(Mathf.Sin(orbitAngle), Mathf.Cos(orbitAngle)) * orbitRange;
		float distanceFromTarget = Vector2.Distance(currentPos, enemyPos);

		Vector2 direction = enemyPos - currentPos;
		int count = Physics2D.RaycastNonAlloc(currentPos, direction, lineOfSight, distanceFromTarget);
		GoToLocation(enemyPos + orbitPos, count > 1, 0.2f, true,
			distanceFromTarget > firingRange ? null : (Vector2?)targetEntity.transform.position - transform.right);

		//fire will in range
		readyToFire = distanceFromTarget <= firingRange;
		if (readyToFire)
		{
			straightWeapon.aim = targetEntity.transform.position;
		}

		//run away if hp drops below 50% while also having lower health than the target
		if (!IsSwarmInCombat)
		{
			float hpRatio = healthComponent.CurrentRatio;
			if (hpRatio < 0.5f && hpRatio < targetEntity.HealthRatio)
			{
				scaryEntities.Add(targetEntity);
				SetState(AIState.Signalling);
			}
		}

	}

	protected virtual void Collecting()
	{

	}

	protected virtual void Escaping()
	{
		Vector2 targetPos = Vector2.zero;
		if (IsInCombat)
		{
			Vector2 enemyPos = ((Entity)enemies[0]).transform.position;
			IncrementOutOfRangeCounter(SiblingsInRangeOfTarget(enemyPos));
			float enemyDistance = Vector2.Distance(transform.position, enemyPos);
			if (hive)
			{
				targetPos = hive.transform.position;
				if (GoToLocation(targetPos, false, 3f, false) && enemyDistance < firingRange)
				{
					SetState(AIState.Attacking);
				}
			}
			else
			{
				targetPos = ((Vector2)transform.position - enemyPos) * 5f;
				GoToLocation(targetPos, false, 3f, false);
			}
		}
		else
		{
			SetState(AIState.Storing);
		}
	}

	protected virtual void Wandering()
	{

	}

	protected virtual void Dying()
	{
		dyingTimer += Time.deltaTime;
		bool meetsMinimumTime = dyingTimer >= minimumDyingDuration;
		bool isInView = CameraControl.IsInView(gameObject);
		bool meetsMaximumTime = dyingTimer >= maximumDyingDuration;
		if ((meetsMinimumTime && isInView) || meetsMaximumTime)
		{
			DestroySelf(destroyer, dropModifier);
		}
	}

	protected virtual void SetState(AIState newState)
	{
		if (state == AIState.Dying || state == newState) return;
		bool wasIdle = isIdle;
		isIdle = true;
		bool couldDrill = canDrill;
		canDrill = false;

		switch (newState)
		{
			case AIState.Suspicious:
				suspicionMeter = suspicionMeter ?? Instantiate(radialMeterPrefab);
				suspicionMeter.transform.parent = ParticleGenerator.holder;
				suspicionMeter.material.SetFloat("_ArcAngle", 0);
				break;
			case AIState.Gathering:
				canDrill = true;
				isIdle = false;
				break;
			case AIState.Spawning:
				transform.position = hive.GetDock(this).position;
				hive.Store(DefaultInventory.ItemStacks, this);
				itemsCollected = 0;
				disassembling = false;
				unsuspiciousEntities.Clear();
				break;
			case AIState.Signalling:
				for (int i = 0; i < hive.childBots.Count; i++)
				{
					GatherBot sibling = hive.childBots[i];
					if (sibling == null)
					{
						hive.childBots.Remove(sibling);
					}
					if (sibling == this) continue;
					float scanAngle = -Vector2.SignedAngle(Vector2.up, sibling.transform.position - transform.position);
					StartCoroutine(ScanRings(scanAngle, 30f, false, 0.3f));
				}
				signalTimer = scanDuration;
				break;
			case AIState.Attacking:
				anim.SetTrigger("GunOut");
				isIdle = false;
				break;
			case AIState.Exploring:
				hive.AssignUnoccupiedCoords(this);
				break;
			case AIState.Escaping:
				outOfRangeTimer = 0f;
				break;
			case AIState.Dying:
				dyingTimer = 0f;
				anim.enabled = false;
				sprRend.sprite = dyingSprite;
				burningEffectsObj?.SetActive(true);
				ActivateAllColliders(false);
				EjectFromAllDrillers(true);
				break;
		}

		if (isIdle && isIdle != wasIdle)
		{
			anim.SetTrigger("Idle");
		}

		if (canDrill && canDrill != couldDrill)
		{
			anim.SetTrigger("DrillOut");
		}

		if (newState != AIState.Gathering)
		{
			drill.StopDrilling(false);
			anim.SetBool("Drilling", false);
		}

		if (newState != AIState.Suspicious)
		{
			nearbySuspects.Clear();
			intenseScanner.Stop();
			suspicionMeter?.gameObject.SetActive(false);
			suspicionMeter?.material.SetFloat("_ArcAngle", 0);
		}

		state = newState;
	}

	protected bool GoToLocation(Vector2 targetPos, bool avoidObstacles = true, float distLimit = 1f,
		bool adjustForMomentum = false, Vector2? lookPos = null)
	{
		float distLeft = Vector2.Distance(transform.position, targetPos);
		float rotTo = 0f;
		if (distLeft > distLimit)
		{
			float expectedAngle = DetermineDirection(targetPos, avoidObstacles);
			if (adjustForMomentum)
			{
				expectedAngle = AdjustForMomentum(expectedAngle);
			}
			if (lookPos == null)
			{
				rotTo = RotateTo(expectedAngle);
			}
			float speedMod = 1f - rotTo;
			DetermineAcceleration(expectedAngle, speedMod, distLeft, distLimit);
		}

		if (lookPos != null)
		{
			float lookAngle = -Vector2.SignedAngle(Vector2.up, targetEntity.transform.position - transform.position);
			rotTo = RotateTo(lookAngle);
		}

		return distLeft <= distLimit;
	}

	public void HiveOrders(Vector2 pos) => targetLocation = pos;

	public ChunkCoords GetIntendedCoords()
	{
		if (state == AIState.Exploring || state == AIState.Spawning)
		{
			return new ChunkCoords(targetLocation, EntityNetwork.CHUNK_SIZE);
		}
		return coords;
	}

	private void SearchForNearestAsteroid()
	{
		int searchRange = 1;

		Asteroid closestAsteroid = null;
		float closestDistance = float.PositiveInfinity;
		while (closestAsteroid == null)
		{
			EntityNetwork.IterateEntitiesInRange(
				coords,
				searchRange,
				e =>
				{
					if (e is Asteroid asteroid)
					{
						if ((hive != null && hive.VerifyGatheringTarget(this, e))
						    || hive == null)
						{
							float dist = Vector2.Distance(transform.position, e.transform.position);
							if (dist < closestDistance)
							{
								closestDistance = dist;
								closestAsteroid = asteroid;
							}
						}
					}

					return false;
				});
			searchRange++;
		}

		targetEntity = closestAsteroid;
		searchTimer = TimeController.TimeSinceOpen;
	}

	private float AdjustForMomentum(float lookDir)
	{
		float ld = lookDir, rt = -Vector2.SignedAngle(Vector2.up, velocity);

		float difference = ld - rt;
		if (difference > 180f)
		{
			difference -= 360f;
		}
		else if (difference < -180f)
		{
			difference += 360f;
		}

		float delta = 1f - Mathf.Abs(difference) / 180f;
		delta = Mathf.Pow(delta, 1f / velocity.magnitude);
		ld += difference * delta;
		while (ld < 0f)
		{
			ld += 360f;
		}

		return ld;
	}

	private void DetermineAcceleration(float expectedAngle, float speedMod, float? distLeft = null,
		float distLimit = 1f)
	{
		if (distLeft != null)
		{
			if (distLeft < distLimit)
			{
				speedMod *= (float)distLeft / distLimit;
			}
		}
		float mag = engineStrength * speedMod;
		float topAccel = Mathf.Min(engineStrength, speedLimit);
		accel = new Vector2(Mathf.Sin(Mathf.Deg2Rad * expectedAngle), Mathf.Cos(Mathf.Deg2Rad * expectedAngle))
			* Mathf.Min(mag, topAccel);
	}

	private float RotateTo(float angle)
	{
		float rotMod = Mathf.Abs(rot - angle);
		if (rotMod > 180f)
		{
			rotMod = Mathf.Abs(rotMod - 360f);
		}
		rotMod /= 180f;
		rotMod = Mathf.Pow(rotMod, 0.8f);
		rot = (Mathf.MoveTowardsAngle(rot, angle, rotationSpeed * rotMod * Time.deltaTime * 60f) + 360f) % 360f;
		return rotMod;
	}

	private float DetermineDirection(Vector2 targetPos, bool avoidObstacles = false)
	{
		float angleTo = -Vector2.SignedAngle(Vector2.up, targetPos - (Vector2)transform.position);

		if (avoidObstacles)
		{
			return RaycastDivert(angleTo, Vector2.Distance(transform.position, targetPos));
		}
		else
		{
			return angleTo;
		}
	}

	private Vector2[] dirs = new Vector2[5];
	private List<RaycastHit2D> hits = new List<RaycastHit2D>();
	private RaycastHit2D[] checks = new RaycastHit2D[2];
	private float RaycastDivert(float angleTo, float DistLeft)
	{
		dirs[0] = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo - 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo - 30f)));
		dirs[1] = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo - 15f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo - 15f)));
		dirs[2] = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angleTo), Mathf.Cos(Mathf.Deg2Rad * angleTo));
		dirs[3] = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + 15f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + 15f)));
		dirs[4] = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + 30f)));

		hits.Clear();
		float dist = Mathf.Min(distanceCheck, DistLeft);
		for (int i = 0; i < dirs.Length; i++)
		{
			dirs[i].Normalize();
			Debug.DrawLine(transform.position, (Vector2)transform.position + dirs[i] * dist);
			int hitCount = Physics2D.RaycastNonAlloc(transform.position, dirs[i], checks, dist);
			hits.Add(GetClosestHit(checks, hitCount));
		}

		if (FacingWall(hits))
		{
			//print("facing wall");
			return angleTo + 135f;
		}

		float change = 0f;
		for (int i = 0; i < hits.Count; i++)
		{
			RaycastHit2D hit = hits[i];
			if (hit.collider != null && !IsTarget(hit))
			{
				accel *= hit.fraction;
				float delta = 1f - hit.fraction;
				switch (i)
				{
					case 0:
						change += maxSway * delta;
						break;
					case 1:
						change += maxSway * delta * 1.5f;
						break;
					case 2:
						change += maxSway * delta * 2f;
						break;
					case 3:
						change -= maxSway * delta * 1.5f;
						break;
					case 4:
						change -= maxSway * delta;
						break;
				}
			}
		}
		Debug.DrawLine(transform.position, (Vector2)transform.position + new Vector2(
			Mathf.Sin(Mathf.Deg2Rad * (angleTo + change)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + change))),
			Color.red);
		return angleTo + change;
	}

	private List<Rigidbody2D> obstacles = new List<Rigidbody2D>();
	private int[] obstacleCounts = new int[5];
	private bool FacingWall(List<RaycastHit2D> hits)
	{
		//look at all the objects being seen by the raycasts
		//if there are too many hits on the same object then assume it is a wall or large object
		obstacles.Clear();
		for (int i = 0; i < hits.Count; i++)
		{
			RaycastHit2D hit = hits[i];
			if (hit.collider == null) continue;
			obstacles.Add(hit.collider.attachedRigidbody);
		}
		//clear counters
		for (int i = 0; i < obstacleCounts.Length; i++)
		{
			obstacleCounts[i] = 0;
		}

		for (int i = 0; i < obstacles.Count; i++)
		{
			for (int j = 0; j < obstacles.Count; j++)
			{
				if (obstacles[i] == obstacles[j])
				{
					if (obstacleCounts[i] >= 2) return true;
					obstacleCounts[i] += 1;
				}
			}
		}
		return false;
	}

	private RaycastHit2D GetClosestHit(RaycastHit2D[] hits, int hitCount)
	{
		RaycastHit2D closest = new RaycastHit2D();
		float distance = Mathf.Infinity;
		for (int i = 0; i < hitCount && i < hits.Length; i++)
		{
			if (hits[i].collider == null) continue;
			if (hits[i].collider.attachedRigidbody == rb) continue;
			float dist = Vector2.Distance(transform.position, hits[i].collider.attachedRigidbody.transform.position);
			if (dist < distance)
			{
				closest = hits[i];
				distance = dist;
			}
		}
		return closest;
	}

	private bool IsSuspicious(ICombat threat)
	{
		if (threat == null || threat == GetICombat()) return false;

		Entity e = (Entity)threat;
		//ignore if too far away
		if (Vector2.Distance(transform.position, e.transform.position) > suspiciousChaseRange) return false;

		for (int i = 0; i < unsuspiciousEntities.Count; i++)
		{
			if (unsuspiciousEntities[i] == e) return false;
		}

		//determine suspect based on entity type
		EntityType type = e.EntityType;
		if (type == EntityType.Shuttle) return true;
		if (type == EntityType.BotHive) return (Entity)threat != hive;
		if (type == EntityType.GatherBot) return !IsSibling(e);

		return false;
	}

	private bool IsScary(Entity e)
	{
		for (int i = 0; i < scaryEntities.Count; i++)
		{
			if (scaryEntities[i] == e) return true;
		}
		return false;
	}

	private bool IsTarget(RaycastHit2D hit)
	{
		if (targetEntity == null) return false;
		GameObject obj = hit.collider.gameObject;
		while (obj != null)
		{
			if (obj == targetEntity.gameObject)
			{
				return true;
			}

			if (obj.transform.parent == null) break;

			obj = obj.transform.parent.gameObject;
		}
		return false;
	}

	private void ApplyMovementCalculation()
	{
		float speedCheck = CheckSpeed();
		float decelerationModifier = 1f;
		if (speedCheck > 1f)
		{
			decelerationModifier *= speedCheck;
		}

		if (IsDrilling)
		{
			//freeze constraints
			rb.constraints = RigidbodyConstraints2D.FreezeAll;
			//add potential velocity
			velocity += accel / 10f;
			//apply a continuous slowdown effect
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, 0.1f);
			//set an upper limit so that the drill speed doesn't go too extreme
			if (speedCheck > 1f)
			{
				velocity.Normalize();
				velocity *= speedLimit;
			}
			rot = -transform.eulerAngles.z;
		}
		else
		{
			velocity = rb.velocity;
			//keep constraints unfrozen
			rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			rb.drag = Mathf.MoveTowards(
				rb.drag,
				deceleration * decelerationModifier,
				decelEffectiveness);
			transform.eulerAngles = Vector3.forward * -rot;
		}
	}

	private IEnumerator ScanRings(float angle = 0f, float arcSize = 360f, bool loop = true, float frequency = 0.5f)
	{
		WaitForSeconds wfs = new WaitForSeconds(frequency);
		System.Action a = () =>
		{
			ExpandingCircle scan = Instantiate(scanningBeam);
			scan.lifeTime = scanDuration;
			scan.rot = angle;
			scan.arcSize = arcSize;
			scan.loop = loop;
			scan.transform.position = transform.position;
			scan.transform.parent = ParticleGenerator.holder;
		};
		a();
		yield return wfs;
		a();
		yield return wfs;
		a();
	}

	private float CheckSpeed() => velocity.magnitude / speedLimit;

	public void Create(BotHive botHive, float maxHp, int dockingID)
	{
		hive = botHive;
		SetState(AIState.Spawning);
		healthComponent.SetUpperLimit(maxHp, false);
		healthComponent.SetToUpperLimit();
		dockID = dockingID;
	}

	public void Activate(bool active)
	{
		activated = active;
		ActivateRenderers(activated && CheckInCameraViewRange());
		ActivateAllColliders(active);
	}

	protected override bool ShouldBeVisible() => activated;

	protected virtual IEnumerator ChargeForcePulse()
	{
		if (!beingDrilled) yield break;
		
		float timer = 0f;
		Color clearWhite = Color.white;
		clearWhite.a = 0f;
		while (timer < chargeToExplosionTimer)
		{
			if (!beingDrilled && timer / chargeToExplosionTimer < 0.5f) break;
			timer += Time.deltaTime;
			chargeSprRend.color = Color.Lerp(clearWhite, Color.white, timer / chargeToExplosionTimer);
			chargeSprRend.transform.localScale = Vector3.one
				* Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(timer / chargeToExplosionTimer, 0.8f));
			yield return null;
		}
		if (timer / chargeToExplosionTimer > 0.5f)
		{
			chargeSprRend.color = clearWhite;
			chargeSprRend.transform.localScale = Vector3.one;
			ForcePulseExplosion();
		}
		else
		{
			while (chargeSprRend.color.a > 0.05f)
			{
				chargeSprRend.color = Color.Lerp(chargeSprRend.color, clearWhite, 0.3f);
				yield return null;
			}
			chargeSprRend.color = clearWhite;
			chargeSprRend.transform.localScale = Vector3.one;
		}
	}

	public void ForcePulseExplosion()
	{
		Instantiate(forcePulseWave, transform.position, Quaternion.identity, ParticleGenerator.holder);
		Vector2 point = transform.position;
		int layers = (1 << LayerSolid) | (1 << LayerProjectile);
		Collider2D[] colliders = Physics2D.OverlapCircleAll(point, explosionRadius, layers);
		List<Rigidbody2D> rbs = new List<Rigidbody2D>();
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider2D col = colliders[i];
			if (col.attachedRigidbody.bodyType == RigidbodyType2D.Static) continue;
			bool found = false;
			for (int j = 0; j < rbs.Count; j++)
			{
				Rigidbody2D colRb = rbs[j];
				if (colRb == col.attachedRigidbody)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				rbs.Add(col.attachedRigidbody);
			}
		}

		for (int i = 0; i < rbs.Count; i++)
		{
			Rigidbody2D colRb = rbs[i];
			IStunnable stunnable = colRb.GetComponent<IStunnable>();
			stunnable = stunnable ?? colRb.GetComponentInChildren<IStunnable>();
			stunnable?.Stun();

			if (colRb == rb) continue;
			Vector2 dir = ((Vector2)colRb.transform.position - point).normalized;
			float distance = Vector2.Distance(point, colRb.transform.position);
			if (distance >= explosionRadius) continue;
			colRb.velocity += dir * Mathf.Pow((explosionRadius - distance) / explosionRadius, 0.5f)
				* explosionStrength;
			colRb.AddTorque((UnityEngine.Random.value > 0.5 ? 1f : -1f) * explosionStrength * 5f);
		}

		Vector2 screenPos = Camera.main.WorldToViewportPoint(transform.position);
		if (screenPos.x > -0.5f || screenPos.x < 1.5f || screenPos.y > -0.5f || screenPos.y < 1.5f)
		{
			screenRippleSO.StartRipple(this, distortionLevel: 0.03f,
				position: screenPos);
		}
	}

	public override void StartDrilling(DrillBit db)
	{
		base.StartDrilling(db);

		beingDrilled = true;
		if (IsDrilling)
		{
			drill.StopDrilling(false);
			drill.drillTarget?.StopDrilling(drill);
		}
		shakeFX.Begin();
		TimeController.DelayedAction(() =>
		{
			if (this == null) return;
			StartCoroutine(ChargeForcePulse());
		}, drillToChargeTimer, true);
	}

	public override void StopDrilling(DrillBit db)
	{
		base.StopDrilling(db);
		beingDrilled = false;
		shakeFX.Stop();
	}

	protected override void LaunchImpact(float angle, Vector2 contactPoint, Collider2D other)
	{
		base.LaunchImpact(angle, contactPoint, other);
		Stun();
	}

	//returns whether the entity is a sibling gather bot (bot produced by the same hive)
	private bool IsSibling(Entity e) => 
		e.EntityType== EntityType.GatherBot
		&& hive != null
		&& ((GatherBot)e).hive == hive;

	private bool IncrementOutOfRangeCounter(bool reset)
	{
		if (reset)
		{
			outOfRangeTimer = 0f;
			return false;
		}

		outOfRangeTimer += Time.deltaTime;
		if (outOfRangeTimer >= outOfRangeCountdown)
		{
			outOfRangeTimer = 0f;
			enemies[0].DisengageInCombat(this);
			enemies.RemoveAt(0);
			if (enemies.Count == 0)
			{
				SetState(AIState.Collecting);
				return true;
			}
		}
		return false;
	}

	private bool SiblingsInRangeOfTarget(Vector2 enemyPos)
	{
		for (int i = 0; i < hive?.childBots.Count; i++)
		{
			GatherBot bot = hive.childBots[i];
			float dist = Vector2.Distance(bot.transform.position, enemyPos);
			if (dist < chaseRange) return true;
		}
		return Vector2.Distance(transform.position, enemyPos) < chaseRange;
	}

	public override bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		bool dead = base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
		if (dead) return true;
		//cannot be hit by projectiles from self or siblings
		if (!IsSibling(destroyer) && destroyer != hive)
		{
			ICombat enemy = destroyer.GetICombat();
			if (flash)
			{
				colorReplacement?.Flash(0.5f, Color.white);
			}
			if (enemy != null)
			{
				if (enemy.EngageInCombat(this))
				{
					AlertAll(enemy);
				}
			}
		}
		return false;
	}

	public override void DrillComplete()
	{
		waitingForResources = true;
		bool hiveOrders = hive?.SplitUpGatheringUnits(this) ?? false;
		drillCount++;

		if (drillCount < drillLimit && !hiveOrders) return;

		waitingForResources = false;
		drillCount = 0;
		targetEntity = null;
		if (hiveOrders)
		{
			SetState(AIState.Exploring);
		}
		else
		{
			SetState(AIState.Scanning);
		}
	}

	public override bool VerifyDrillTarget(Entity target) =>
		target == targetEntity
		&& target != hive
		&& !IsSibling(target);

	protected override bool CheckHealth(Entity destroyer, float dropModifier)
	{
		if (healthComponent.CurrentRatio > 0f) return false;
		if (IsDrilling)
		{
			drill.drillTarget.StopDrilling(drill);
		}
		SetState(AIState.Dying);
		this.destroyer = destroyer;
		this.dropModifier = dropModifier;
		return healthComponent.CurrentRatio <= 0f;
	}

	public override void DestroySelf(Entity destroyer, float dropModifier)
	{
		bool explode = destroyer != null;
		if (explode)
		{
			//particle effects
			GameObject explosion = Instantiate(explosionDeathObj, ParticleGenerator.holder);
			explosion.transform.position = transform.position;

			//sound effects
		}
		for (int i = 0; i < enemies.Count; i++)
		{
			ICombat enemy = enemies[i];
			enemy.DisengageInCombat(this);
		}
		if (hive != null)
		{
			hive.BotDestroyed(this);
		}
		
		base.DestroySelf(destroyer, dropModifier);
	}

	public override float DrillDamageQuery(bool firstHit) => speedLimit;

	public override int GiveItem(ItemStack stack)
	{
		int collectedAmount = stack.Amount- base.GiveItem(stack);
		itemsCollected += collectedAmount;
		if (itemsCollected >= storageCapacity)
		{
			SetState(AIState.Storing);
		}
		waitingForResources = false;
		return collectedAmount;
	}

	public override EntityType EntityType => EntityType.GatherBot;

	protected virtual AttackViability EvaluateScan(Scan sc)
	{
		float baseThreatMultiplier = 1f;
		switch (sc.type)
		{
			case EntityType.Asteroid:
				baseThreatMultiplier = 0f;
				break;
			case EntityType.Shuttle:
				baseThreatMultiplier = 0f;
				break;
			case EntityType.Nebula:
				baseThreatMultiplier = 0f;
				break;
			case EntityType.BotHive:
				baseThreatMultiplier = 5f;
				break;
			case EntityType.GatherBot:
				baseThreatMultiplier = 3f;
				break;
			default:
				baseThreatMultiplier = 1f;
				break;
		}

		float targetThreatValue = baseThreatMultiplier * sc.level * sc.hpRatio * 1.5f;
		float gatherBotBackupModifier = hive?.childBots.Count ?? 1f;
		float selfThreatValue = GetLevel() * healthComponent.CurrentRatio;
		float swarmThreatValue = selfThreatValue * gatherBotBackupModifier;
		bool isValuable = sc.value >= valuableLootThreshold;

		if (selfThreatValue >= targetThreatValue)
		{
			return isValuable ? AttackViability.AttackAlone : AttackViability.Ignore;
		}		else if (swarmThreatValue >= targetThreatValue)
		{
			return isValuable ? AttackViability.SignalForHelp : AttackViability.Ignore;
		}
		else
		{
			return AttackViability.Escape;
		}
	}

	protected enum AttackViability { AttackAlone, SignalForHelp, Escape, Ignore }

	public override void DestroyedAnEntity(Entity target)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if ((Entity)enemies[i] == target)
			{
				enemies[i].DisengageInCombat(this);
				enemies.RemoveAt(i);
				if (enemies.Count == 0)
				{
					for (int j = 0; j < hive?.childBots.Count; j++)
					{
						GatherBot bot = hive.childBots[j];
						bot?.SetState(AIState.Collecting);
					}
				}
				return;
			}
		}
	}

	public override void Launch(Vector2 launchDirection, Character launcher)
	{
		base.Launch(launchDirection, launcher);
		shakeFX.Begin(0.1f, 0f, 1f / 30f);
	}

	public void Stun()
	{
		stunned = true;
		stunTimer = stunDuration;
	}

	public override bool IsDrillable() => base.IsDrillable() && state != AIState.Dying;

	public void Alert(ICombat threat)
	{
		AddThreat(threat);
		SetState(AIState.Attacking);
	}

	protected virtual void AlertAll(ICombat threat)
	{
		for (int i = 0; i < hive?.childBots.Count; i++)
		{
			GatherBot sibling = hive.childBots[i];
			sibling.AddThreat(threat);
			sibling.SetState(AIState.Attacking);
		}
	}

	private void AddThreat(ICombat threat)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			ICombat e = enemies[i];
			if (e == threat) return;
		}
		enemies.Add(threat);
	}

	public bool EngageInCombat(ICombat hostile)
	{
		if (IsSibling((Entity)hostile) || (Entity)hostile == hive) return false;

		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] == hostile) return false;
		}
		AddThreat(hostile);
		return true;
	}

	public void DisengageInCombat(ICombat nonHostile)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] == nonHostile)
			{
				enemies.RemoveAt(i);
				return;
			}
		}
	}

	public override bool CanFireStraightWeapon() =>
		straightWeaponAttached
		&& readyToFire
		&& !beingDrilled
		&& !stunned
		&& state == AIState.Attacking;

	public override void AttachStraightWeapon(bool attach) => straightWeaponAttached = attach;

	public override ICombat GetICombat() => this;

	public GatherBotData GetData() => new GatherBotData();

	public void ApplyData(GatherBotData data)
	{

	}

	[Serializable]
	public struct GatherBotData
	{

	}
}