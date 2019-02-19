using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GatherBot : Entity, IDrillableObject, IDamageable, IStunnable, ICombat
{
	private enum AIState
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
		Escaping
	}

	//references
	private BotHive hive;
	[SerializeField]
	private ShakeEffect shakeFX;
	[SerializeField]
	private Inventory storage;
	[SerializeField]
	private Animator anim;
	[SerializeField]
	private AudioSO collisionSounds;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];

	//fields
	[SerializeField]
	private AIState state;
	private float maxHealth = 2000f;
	private float currentHealth;
	[HideInInspector]
	public int upgradeLevel;
	public int dockID = -1;

	//movement variables
	private Vector2 accel;
	[SerializeField]
	private float engineStrength = 1f, speedLimit = 2f, deceleration = 2f, decelEffectiveness = 0.01f,
		rotationSpeed = 3f;
	private float rot;
	private Vector2 velocity;
	private float maxSway = 45;
	private float distanceCheck = 10f;
	public Entity targetEntity;

	//scanning variables
	[SerializeField]
	private ExpandingCircle scanningBeam;
	private float scanTimer, scanDuration = 3f;
	private bool scanStarted;
	private List<Entity> entitiesScanned = new List<Entity>();
	private bool isIdle = true;

	//gathering variables
	private float searchTimer, scanInterval = 0.3f;
	private int drillCount, drillLimit = 3;
	private List<Entity> surroundingEntities = new List<Entity>();
	private IDrillableObject drillableTarget;
	[SerializeField]
	private int storageCapacity = 10;
	private int itemsCollected;
	private bool waitingForResources;

	//exploring variables
	private bool waitingForHiveDirection = true;
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
	[SerializeField]
	private ParticleSystem intenseScanner;
	[SerializeField]
	private SpriteRenderer radialMeterPrefab;
	private SpriteRenderer suspicionMeter;
	[SerializeField]
	private Vector2 meterRelativePosition = Vector2.one * 0.2f;

	//combat variables
	[SerializeField]
	private StraightWeapon straightWeapon;
	private bool beingDrilled;
	private List<ICombat> enemies = new List<ICombat>();
	private float chaseRange = 16f;
	[SerializeField]
	private float outOfRangeCountdown = 8f;
	private float outOfRangeTimer;
	[SerializeField]
	private float orbitRange = 1.75f;
	[SerializeField]
	private float orbitSpeed = 0.6f;
	[SerializeField]
	private float firingRange = 5f;
	[SerializeField]
	private float drillToChargeTimer = 1f;
	[SerializeField]
	private float chargeToExplosionTimer = 3f;
	[SerializeField]
	private SpriteRenderer chargeSprRend;
	[SerializeField]
	private float explosionRadius = 3f;
	[SerializeField]
	private float explosionStrength = 1f;
	private bool stunned = false;
	[SerializeField]
	private float stunDuration = 2f;
	private float stunTimer = 0f;
	private bool launched = false;
	private float launchedDuration = 1f;
	private Entity launcher;
	private LaunchTrailController launchTrail;
	public float drillDamageResistance = 2f;
	[SerializeField]
	private ExpandingCircle forcePulseWave;
	[SerializeField]
	private List<Loot> loot;
	private bool straightWeaponAttached = false;
	private bool readyToFire = false;

	//signalling variables
	private float signalTimer;

	private void Start()
	{
		if (!hive) DestroySelf(false, null);
		rot = transform.eulerAngles.z;
		initialised = true;
	}

	private void Update()
	{
		accel = Vector2.zero;
		if (stunned)
		{
			stunTimer -= Time.deltaTime;
			stunned = stunTimer <= 0f;
		}
		if (beingDrilled || launched) return;

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
		}

		ApplyMovementCalculation();
	}

	private void FixedUpdate()
	{
		Rb.AddForce(accel);
	}

	#region State Methods

	private void Spawning()
	{
		Transform dock = hive.GetDock(this);
		Vector2 targetPos = dock.position - dock.up * dockDistance;
		if (GoToLocation(targetPos))
		{
			if (Rb.velocity.sqrMagnitude < Mathf.Epsilon)
			{
				StartExploring(true);
			}
		}
	}

	private void Scanning()
	{
		if (Rb.velocity.sqrMagnitude < Mathf.Epsilon)
		{
			if (!scanStarted)
			{
				if (isActive)
				{
					StartCoroutine(ScanRings());
				}
				scanStarted = true;
				entitiesScanned.Clear();
				new Thread(() =>
				{
					EntityNetwork.GetEntitiesInRange(_coords, 1,
						exclusions: new List<Entity> { this }, addToList: entitiesScanned);
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
				foreach (Entity e in entitiesScanned)
				{
					ICombat threat = e.GetICombat();
					if (threat != null && IsSuspicious(threat))
					{
						nearbySuspects.Add(threat);
					}
				}

				if (nearbySuspects.Count > 0)
				{
					state = AIState.Suspicious;
					if (suspicionMeter == null)
					{
						suspicionMeter = Instantiate(radialMeterPrefab);
						suspicionMeter.transform.parent = ParticleGenerator.holder;
						suspicionMeter.material.SetFloat("_ArcAngle", 0);
					}
					if (!isIdle)
					{
						anim.SetTrigger("Idle");
						isIdle = true;
					}
					return;
				}

				//choose state
				if (entitiesScanned.Count >= 10)
				{
					state = AIState.Gathering;
					canDrill = true;
					anim.SetTrigger("DrillOut");
					isIdle = false;
				}
				else
				{
					hive.MarkCoordAsEmpty(_coords);
					StartExploring();
					if (!isIdle)
					{
						anim.SetTrigger("Idle");
						isIdle = true;
					}
				}
			}
		}
	}

	private void Gathering()
	{
		if (waitingForResources) return;

		if (Pause.timeSinceOpen - searchTimer > scanInterval || targetEntity == null)
		{
			SearchForNearestAsteroid();
		}
		Vector2 targetPos = targetEntity.transform.position;
		if (targetEntity.disabled)
		{
			GoToLocation(targetPos, false, 1f, true);
			float distLeft = Vector2.Distance(transform.position, targetPos);
			if (distLeft <= 1f)
			{
				if (drillableTarget != null && drillableTarget.TakeDrillDamage(DrillDamageQuery(false), drill.transform.position, this))
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

	private void Exploring()
	{
		if (!waitingForHiveDirection)
		{
			if (GoToLocation(targetLocation, isInPhysicsRange))
			{
				state = AIState.Scanning;
			}
		}
	}

	private void Storing()
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
					transform.position = dock.position;
					hive.Store(storage.inventory, this);
					itemsCollected = 0;
					state = AIState.Spawning;
					disassembling = false;
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

	private void Suspicious()
	{
		if (nearbySuspects.Count == 0)
		{
			StartExploring();
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
				int threatLevel = EvaluateScan(targetEntity.ReturnScan());
				switch (threatLevel)
				{
					//attack alone
					case 0:
						ICombat enemy = targetEntity.GetICombat();
						if (enemy != null)
						{
							if (enemy.EngageInCombat(this))
							{
								AddThreat(enemy);
							}
						}
						state = AIState.Attacking;
						nearbySuspects.Clear();
						break;
					//signal for help
					case 1:
						enemy = targetEntity.GetICombat();
						if (enemy != null)
						{
							if (enemy.EngageInCombat(this))
							{
								AddThreat(enemy);
							}
						}
						state = AIState.Signalling;
						nearbySuspects.Clear();
						foreach (GatherBot sibling in hive.childBots)
						{
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
					//escape
					case 2:
						hive.MarkCoordAsEmpty(targetEntity.GetCoords());
						StartExploring();
						nearbySuspects.Clear();
						break;
					//ignore
					case 3:
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

	private void Signalling()
	{
		signalTimer -= Time.deltaTime;
		if (signalTimer <= 0f)
		{
			foreach (GatherBot sibling in hive.childBots)
			{
				sibling.enemies = enemies;
				sibling.StartEmergencyAttack();
				foreach (ICombat enemy in enemies)
				{
					enemy.EngageInCombat(sibling);
				}
			}
		}
	}

	private void Attacking()
	{
		//if this bot is too far away and all other bots are also too far away then give up chase
		float distanceFromTarget = Vector2.Distance(transform.position, ((Entity)enemies[0]).transform.position);
		if (distanceFromTarget > chaseRange)
		{
			bool found = false;
			foreach (GatherBot bot in hive.childBots)
			{
				if (bot == this || bot == null) continue;
				float dist = Vector2.Distance(bot.transform.position, ((Entity)enemies[0]).transform.position);
				if (dist < chaseRange)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				outOfRangeTimer += Time.deltaTime;
				if (outOfRangeTimer >= outOfRangeCountdown)
				{
					outOfRangeTimer = 0f;
					enemies[0].DisengageInCombat(this);
					enemies.RemoveAt(0);
					if (enemies.Count == 0)
					{
						foreach (GatherBot bot in hive.childBots)
						{
							bot.state = AIState.Collecting;
							bot.anim.SetTrigger("Idle");
							bot.isIdle = true;
						}
						return;
					}
				}
			}
			else
			{
				outOfRangeTimer = 0f;
			}
		}
		else
		{
			outOfRangeTimer = 0f;
		}

		foreach (GatherBot sibling in hive.childBots)
		{
			if (sibling.state == AIState.Attacking) continue;

			if (Vector2.Distance(transform.position, sibling.transform.position)
				< Constants.CHUNK_SIZE)
			{
				float scanAngle = -Vector2.SignedAngle(Vector2.up, sibling.transform.position - transform.position);
				StartCoroutine(ScanRings(scanAngle, 30f, false, 0.3f));
				sibling.enemies = enemies;
				sibling.StartEmergencyAttack();
				enemies[0].EngageInCombat(sibling);
			}
		}
		targetEntity = (Entity)enemies[0];
		float orbitAngle = Mathf.PI * 2f / hive.childBots.Count * dockID + Pause.timeSinceOpen * orbitSpeed;
		Vector3 targetPos = new Vector2(Mathf.Sin(orbitAngle), Mathf.Cos(orbitAngle)) * orbitRange;
		GoToLocation(targetEntity.transform.position + targetPos, distanceFromTarget > firingRange, 0.2f, true,
			distanceFromTarget > firingRange ? null : (Vector2?)targetEntity.transform.position - transform.right);
		readyToFire = distanceFromTarget <= firingRange;
		if (readyToFire)
		{
			straightWeapon.aim = targetEntity.transform.position;
		}
		
	}

	private void Collecting()
	{

	}

	private void Escaping()
	{

	}

	#endregion

	private bool GoToLocation(Vector2 targetPos, bool avoidObstacles = true, float distLimit = 1f,
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

	public void HiveOrders(Vector2 pos)
	{
		waitingForHiveDirection = false;
		targetLocation = pos;
	}

	public ChunkCoords GetIntendedCoords()
	{
		if ((state == AIState.Exploring && !waitingForHiveDirection) ||
			(state == AIState.Spawning && !waitingForHiveDirection))
		{
			return new ChunkCoords(targetLocation);
		}
		else
		{
			return _coords;
		}
	}

	private void SearchForNearestAsteroid()
	{
		int searchRange = 1;
		surroundingEntities.Clear();

		while (surroundingEntities.Count == 0)
		{
			EntityNetwork.GetEntitiesInRange(_coords, searchRange, EntityType.Asteroid, addToList: surroundingEntities);

			for (int i = 0; i < surroundingEntities.Count; i++)
			{
				if (!hive.VerifyGatheringTarget(this, surroundingEntities[i]))
				{
					surroundingEntities.RemoveAt(i);
					i--;
				}
			}

			searchRange++;
		}

		float shortestDist = float.PositiveInfinity;
		foreach (Entity e in surroundingEntities)
		{
			float dist = Vector2.Distance(transform.position, e.transform.position);
			if (dist < shortestDist || float.IsPositiveInfinity(shortestDist))
			{
				shortestDist = dist;
				targetEntity = e;
				drillableTarget = targetEntity.GetComponent<IDrillableObject>();
			}
		}

		searchTimer = Pause.timeSinceOpen;
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
		foreach (RaycastHit2D hit in hits)
		{
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
			if (hits[i].collider.attachedRigidbody == Rb) continue;
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
		if (threat == null) return false;

		//ignore if too far away
		if (Vector2.Distance(transform.position, ((Entity)threat).transform.position) > suspiciousChaseRange) return false;

		//determine suspect based on entity type
		EntityType type = ((Entity)threat).GetEntityType();
		if (type == EntityType.Shuttle) return true;
		if (type == EntityType.BotHive) return (Entity)threat != hive;
		if (type == EntityType.GatherBot) return !IsSibling(((Entity)threat));

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
			Rb.constraints = RigidbodyConstraints2D.FreezeAll;
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
			velocity = Rb.velocity;
			//keep constraints unfrozen
			Rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			Rb.drag = Mathf.MoveTowards(
				Rb.drag,
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

	private float CheckSpeed()
	{
		return velocity.magnitude / speedLimit;
	}

	public void Create(BotHive botHive, float MaxHP, int dockingID)
	{
		hive = botHive;
		state = AIState.Spawning;
		maxHealth = MaxHP;
		currentHealth = maxHealth;
		dockID = dockingID;
	}

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0)
	{
		return TakeDamage(drillDmg / drillDamageResistance, drillPos, destroyer, dropModifier);
	}

	public IEnumerator ChargeForcePulse()
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
		int layers = (1 << layerSolid) | (1 << layerProjectile);
		Collider2D[] colliders = Physics2D.OverlapCircleAll(point, explosionRadius, layers);
		List<Rigidbody2D> rbs = new List<Rigidbody2D>();
		foreach (Collider2D col in colliders)
		{
			if (col.attachedRigidbody.bodyType == RigidbodyType2D.Static) continue;
			bool found = false;
			foreach (Rigidbody2D colRb in rbs)
			{
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

		foreach (Rigidbody2D colRb in rbs)
		{
			IStunnable stunnable = colRb.GetComponent<IStunnable>();
			stunnable = stunnable ?? colRb.GetComponentInChildren<IStunnable>();
			if (stunnable != null) stunnable.Stun();

			if (colRb == Rb) continue;
			Vector2 dir = ((Vector2)colRb.transform.position - point).normalized;
			float distance = Vector2.Distance(point, colRb.transform.position);
			if (distance >= explosionRadius) continue;
			colRb.velocity += dir * Mathf.Pow((explosionRadius - distance) / explosionRadius, 0.5f)
				* explosionStrength;
			colRb.AddTorque((Random.value > 0.5 ? 1f : -1f) * explosionStrength * 5f);
		}

		Vector2 screenPos = Camera.main.WorldToViewportPoint(transform.position);
		if (screenPos.x > -0.5f || screenPos.x < 1.5f || screenPos.y > -0.5f || screenPos.y < 1.5f)
		{
			screenRippleSO.StartRipple(this, distortionLevel: 0.03f,
				position: screenPos);
		}
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		beingDrilled = true;
		if (IsDrilling)
		{
			drill.StopDrilling();
			if (drill.drillTarget != null)
			{
				drill.drillTarget.StopDrilling();
			}
		}
		shakeFX.Begin();
		pause = pause ?? FindObjectOfType<Pause>();
		if (pause)
		{
			pause.DelayedAction(() =>
			{
				if (this == null) return;
				StartCoroutine(ChargeForcePulse());
			}, drillToChargeTimer, true);
		}
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		beingDrilled = false;
		shakeFX.Stop();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill && IsDrillable())
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && otherDrill.drillTarget == null && otherDrill.Verify(this))
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();

			if ((Entity)otherDrill.drillTarget == this)
			{
				StopDrilling();
				otherDrill.StopDrilling();
			}
		}
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		Collider2D other = collision.collider;
		int otherLayer = other.gameObject.layer;
		//collision.GetContacts(contacts);
		//Vector2 contactPoint = contacts[0].point;
		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;
		float angle = -Vector2.SignedAngle(Vector2.up, contactPoint - (Vector2)transform.position);

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
		}

		if (otherLayer == layerSolid)
		{
			if (launched)
			{
				if (launcher.GetLaunchImpactAnimation() != null)
				{
					Transform impact = Instantiate(launcher.GetLaunchImpactAnimation()).transform;
					impact.parent = ParticleGenerator.holder;
					impact.position = contactPoint;
					impact.eulerAngles = Vector3.forward * angle;
				}
				if (launchTrail != null)
				{
					launchTrail.CutLaunchTrail();
				}
				IDamageable otherDamageable = other.transform.parent.GetComponent<IDamageable>();
				float damage = launcher.GetLaunchDamage();
				if (currentHealth / maxHealth < 0.5f)
				{
					damage *= 2f;
				}
				if (otherDamageable != null)
				{
					otherDamageable.TakeDamage(damage, contactPoint, launcher);
				}
				TakeDamage(damage, contactPoint, launcher);
				launched = false;
				Stun();
			}
		}
	}

	//returns whether the entity is a sibling gather bot (bot produced by the same hive)
	private bool IsSibling(Entity e)
	{
		if (e.GetEntityType() != EntityType.GatherBot) return false;
		
		return ((GatherBot)e).hive == hive;
	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0)
	{
		//cannot be hit by projectiles from self or siblings
		if (!IsSibling(destroyer))
		{
			currentHealth -= damage;
			ICombat enemy = destroyer.GetICombat();
			if (enemy != null)
			{
				if (enemy.EngageInCombat(this))
				{
					AddThreat(enemy);
				}
			}
			StartEmergencyAttack();
		}
		return CheckHealth(destroyer, dropModifier);
	}

	public override void DrillComplete()
	{
		waitingForResources = true;
		bool hiveOrders = hive.SplitUpGatheringUnits(this);
		drillCount++;

		if (drillCount < drillLimit && !hiveOrders) return;

		waitingForResources = false;
		drillCount = 0;
		targetEntity = null;
		drillableTarget = null;
		if (hiveOrders)
		{
			StartExploring();
		}
		else
		{
			state = AIState.Scanning;
		}
		canDrill = false;
	}

	public override bool VerifyDrillTarget(Entity target)
	{
		return target == targetEntity;
	}

	private bool CheckHealth(Entity destroyer, int dropModifier)
	{
		if (currentHealth > 0f) return false;
		if (IsDrilling)
		{
			drill.drillTarget.StopDrilling();
		}
		destroyer.DestroyedAnEntity(this);
		DestroySelf(true, destroyer, dropModifier);
		return currentHealth <= 0f;
	}

	public void DestroySelf(bool explode, Entity destroyer, int dropModifier = 0)
	{
		if (explode)
		{
			//particle effects

			//sound effects

			//drop resources
			DropLoot(destroyer, transform.position, dropModifier);
		}
		foreach (ICombat enemy in enemies)
		{
			enemy.DisengageInCombat(this);
		}
		if (hive) hive.BotDestroyed(this);
		base.DestroySelf();
	}

	private void DropLoot(Entity destroyer, Vector2 pos, int dropModifier = 0)
	{
		particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();
		if (!particleGenerator) return;

		for (int i = 0; i < storage.inventory.Count; i++)
		{
			ItemStack stack = storage.inventory[i];
			if (stack.GetItemType() == Item.Type.Blank) continue;
			particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
		}

		for (int i = 0; i < loot.Count; i++)
		{
			ItemStack stack = loot[i].GetStack();
			particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
		}
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		return speedLimit;
	}

	public override void CollectResources(Item.Type type, int amount)
	{
		storage.AddItem(type, num: amount);
		itemsCollected += amount;
		if (itemsCollected >= storageCapacity)
		{
			state = AIState.Storing;
			anim.SetTrigger("Idle");
			isIdle = true;
		}
		waitingForResources = false;
	}

	private void StartExploring(bool hasDirections = false)
	{
		state = AIState.Exploring;
		if (!hasDirections)
		{
			waitingForHiveDirection = true;
			hive.AssignUnoccupiedCoords(this);
		}
	}

	private void StartEmergencyAttack()
	{
		if (state == AIState.Attacking) return;

		canDrill = false;
		state = AIState.Attacking;
		anim.SetTrigger("GunOut");
		isIdle = false;
		if (drillableTarget != null)
		{
			drill.StopDrilling();
			anim.SetBool("Drilling", false);
		}
		nearbySuspects.Clear();
		intenseScanner.Stop();
		if (suspicionMeter != null)
		{
			suspicionMeter.gameObject.SetActive(false);
			suspicionMeter.material.SetFloat("_ArcAngle", 0);
		}
	}

	public Vector2 GetPosition()
	{
		return transform.position;
	}

	public override EntityType GetEntityType()
	{
		return EntityType.GatherBot;
	}

	//codes: 0 = attack alone, 1 = signal for help, 2 = escape, 3 = ignore
	private int EvaluateScan(Scan sc)
	{
		return 1;
	}

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
					foreach (GatherBot bot in hive.childBots)
					{
						if (bot == null) continue;

						bot.state = AIState.Collecting;
						bot.anim.SetTrigger("Idle");
						bot.isIdle = true;
					}
				}
				return;
			}
		}
	}

	public void Launch(Vector2 launchDirection, Entity launcher)
	{
		this.launcher = launcher;
		Rb.velocity = launchDirection;
		shakeFX.Begin(0.1f, 0f, 1f / 30f);
		launched = true;
		if (launcher.GetLaunchTrailAnimation() != null)
		{
			launchTrail = Instantiate(launcher.GetLaunchTrailAnimation());
			launchTrail.SetFollowTarget(transform, launchDirection);

		}
		pause = pause ?? FindObjectOfType<Pause>();
		if (pause)
		{
			pause.DelayedAction(() =>
			{
				if (launchTrail != null)
				{
					launchTrail.EndLaunchTrail();
				}
				launched = false;
				this.launcher = null;
			}, launchedDuration, true);
		}
	}

	public void Stun()
	{
		stunned = true;
		stunTimer = stunDuration;
	}

	public bool IsDrillable()
	{
		return true;
	}

	private void AddThreat(ICombat threat)
	{
		foreach (ICombat e in enemies)
		{
			if (e == threat) return;
		}
		enemies.Add(threat);
	}

	public bool EngageInCombat(ICombat hostile)
	{
		if (IsSibling((Entity)hostile) || (Entity)hostile == hive || enemies.Contains(hostile)) return false;
		enemies.Add(hostile);
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

	public override bool CanFireStraightWeapon()
	{
		return straightWeaponAttached && readyToFire && !beingDrilled && !stunned;
	}

	public override void AttachStraightWeapon(bool attach)
	{
		straightWeaponAttached = attach;
	}

	public override ICombat GetICombat()
	{
		return this;
	}
}