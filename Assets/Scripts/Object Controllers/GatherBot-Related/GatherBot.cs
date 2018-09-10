using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GatherBot : Entity, IDrillableObject, IDamageable
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
	private ExpandingCircle scanningBeam;
	[SerializeField]
	private Inventory storage;
	[SerializeField]
	private Animator anim;
	[SerializeField]
	private StraightWeapon straightWeapon;

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
	private float scanTimer, scanDuration = 3f;
	private bool scanStarted;
	private List<Entity> entitiesScanned = new List<Entity>();
	private bool isIdle = true;

	//gathering variables
	private float searchTimer, scanInterval = 0.3f;
	private int drillCount, drillLimit = 3;
	private List<Entity> surroundingEntities;
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
	private List<Entity> nearbySuspects = new List<Entity>();
	[SerializeField]
	private ParticleSystem intenseScanner;

	//combat variables
	private bool beingDrilled;
	private List<Entity> threats = new List<Entity>();
	private float chaseRange = 16f;
	[SerializeField]
	private float orbitRange = 1.75f;
	[SerializeField]
	private float orbitSpeed = 0.6f;
	[SerializeField]
	private float firingRange = 5f;

	//signalling variables
	private float signalTimer;
	private bool respondingToSignal;

	private void Start()
	{
		rot = transform.eulerAngles.z;
		initialised = true;
	}

	private void Update()
	{
		accel = Vector2.zero;

		if (beingDrilled)
		{
			return;
		}

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
					entitiesScanned = EntityNetwork.GetEntitiesInRange(_coords, 1,
						exclusions: new List<Entity> { this });
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
					if (IsSuspicious(e))
					{
						nearbySuspects.Add(e);
					}
				}

				if (nearbySuspects.Count > 0)
				{
					state = AIState.Suspicious;
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
				if (drillableTarget.TakeDrillDamage(DrillDamageQuery(false), drill.transform.position, this))
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
		targetEntity = nearbySuspects[0];
		//if target is too far away then return to exploring
		if (targetEntity == null ||
			Vector2.Distance(targetEntity.transform.position, transform.position) > suspiciousChaseRange)
		{
			intenseScanTimer = 0f;
			nearbySuspects.RemoveAt(0);
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
			float angle = Vector2.Angle(Vector2.up, targetEntity.transform.position - transform.position);
			if (targetEntity.transform.position.x < transform.position.x)
			{
				angle = 180f + (180f - angle);
			}
			intenseScanner.transform.eulerAngles = Vector3.forward * -angle;
			intenseScanner.transform.localScale = Vector3.one *
				Vector2.Distance(transform.position, targetEntity.transform.position);
			//check if scan is complete
			if (intenseScanTimer >= intenseScanDuration)
			{
				intenseScanner.Stop();
				//scan complete
				intenseScanTimer = 0f;
				//assess threat level and make a decision
				int threatLevel = EvaluateScan(targetEntity.ReturnScan());
				switch (threatLevel)
				{
					//attack alone
					case 0:
						threats.Add(targetEntity);
						state = AIState.Attacking;
						nearbySuspects.Clear();
						break;
					//signal for help
					case 1:
						threats.Add(targetEntity);
						state = AIState.Signalling;
						nearbySuspects.Clear();
						foreach (GatherBot sibling in hive.childBots)
						{
							if (sibling == null)
							{
								hive.childBots.Remove(sibling);
							}
							if (sibling == this) continue;
							float scanAngle = Vector2.Angle(
								Vector2.up, sibling.transform.position - transform.position);
							if (sibling.transform.position.x < transform.position.x)
							{
								scanAngle = 180f + (180f - scanAngle);
							}
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
	}

	private void Signalling()
	{
		signalTimer -= Time.deltaTime;
		if (signalTimer <= 0f)
		{
			foreach (GatherBot sibling in hive.childBots)
			{
				sibling.respondingToSignal = true;
				sibling.threats = threats;
				sibling.StartEmergencyAttack();
			}
		}
	}

	private void Attacking()
	{
		//if this bot is too far away and all other bots are also too far away then give up chase
		float distanceFromTarget = Vector2.Distance(transform.position, threats[0].transform.position);
		if (distanceFromTarget > chaseRange && !respondingToSignal)
		{
			bool found = false;
			foreach (GatherBot bot in hive.childBots)
			{
				if (bot == this) continue;
				float dist = Vector2.Distance(bot.transform.position, threats[0].transform.position);
				if (dist < chaseRange)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				threats.RemoveAt(0);
				if (threats.Count == 0)
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

		foreach (GatherBot sibling in hive.childBots)
		{
			if (sibling.state == AIState.Attacking) continue;

			if (Vector2.Distance(transform.position, sibling.transform.position)
				< Cnsts.CHUNK_SIZE)
			{
				float scanAngle = Vector2.Angle(Vector2.up, sibling.transform.position - transform.position);
				if (sibling.transform.position.x < transform.position.x)
				{
					scanAngle = 180f + (180f - scanAngle);
				}
				StartCoroutine(ScanRings(scanAngle, 30f, false, 0.3f));
				sibling.threats = threats;
				sibling.StartEmergencyAttack();
			}
		}
		targetEntity = threats[0];
		float orbitAngle = Mathf.PI * 2f / hive.childBots.Count * dockID + Pause.timeSinceOpen * orbitSpeed;
		Vector3 targetPos = new Vector2(Mathf.Sin(orbitAngle), Mathf.Cos(orbitAngle)) * orbitRange;
		GoToLocation(targetEntity.transform.position + targetPos, distanceFromTarget > firingRange, 0.2f, true,
			distanceFromTarget > firingRange ? null : (Vector2?)targetEntity.transform.position - transform.right);
		if (distanceFromTarget <= firingRange)
		{
			respondingToSignal = false;
			straightWeapon.Fire(targetEntity.transform.position);
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
			float lookAngle = Vector2.Angle(Vector2.up, targetEntity.transform.position - transform.position);
			if (targetEntity.transform.position.x < transform.position.x)
			{
				lookAngle = 180f + (180f - lookAngle);
			}
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
		surroundingEntities = new List<Entity>();

		while (surroundingEntities.Count == 0)
		{
			surroundingEntities = EntityNetwork.GetEntitiesInRange(_coords, searchRange);

			for (int i = 0; i < surroundingEntities.Count; i++)
			{
				if (surroundingEntities[i].GetEntityType() != EntityType.Asteroid
					|| !hive.VerifyGatheringTarget(this, surroundingEntities[i]))
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
		float ld = lookDir, rt = Vector2.Angle(Vector2.up, velocity);
		if (velocity.x < 0f)
		{
			rt = 180f + (180f - rt);
		}

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
		float angleTo = Vector2.Angle(Vector2.up, targetPos - (Vector2)transform.position);
		if (targetPos.x < transform.position.x)
		{
			angleTo = 180f + (180f - angleTo);
		}

		if (avoidObstacles)
		{
			return RaycastDivert(angleTo, Vector2.Distance(transform.position, targetPos));
		}
		else
		{
			return angleTo;
		}
	}

	private float RaycastDivert(float angleTo, float DistLeft)
	{
		Vector2[] dirs = new Vector2[]
		{
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo - 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo - 30f))),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo - 15f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo - 15f))),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * angleTo), Mathf.Cos(Mathf.Deg2Rad * angleTo)),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + 15f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + 15f))),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + 30f)))
		};
		RaycastHit2D[] hits = new RaycastHit2D[dirs.Length];
		float dist = Mathf.Min(distanceCheck, DistLeft);
		for (int i = 0; i < dirs.Length; i++)
		{
			dirs[i].Normalize();
			dirs[i] *= dist;
			Debug.DrawLine((Vector2)transform.position + dirs[i] / (dist * 2f),
				(Vector2)transform.position + dirs[i]);
			hits[i] = Physics2D.Raycast((Vector2)transform.position + dirs[i] / (dist * 2f),
				dirs[i], dist);
		}

		float change = 0f;
		bool facingWall = FacingWall(hits);

		for (int i = 0; i < hits.Length; i++)
		{
			if (facingWall && i != 2) continue;

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
						float obstacleAngle = Vector2.Angle(Vector2.up,
							hit.collider.transform.position - transform.position);
						change += Mathf.MoveTowardsAngle(angleTo, obstacleAngle, -maxSway * delta * 2f) - angleTo;
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

		Debug.DrawLine(transform.position, (Vector2)transform.position
			+ new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + change)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + change))), Color.red);

		return angleTo + change;
	}

	private bool FacingWall(RaycastHit2D[] hits)
	{
		//look at all the objects being seen by the raycasts
		//if there are any duplicates then assume it is a wall
		List<GameObject> obstacles = new List<GameObject>();
		foreach (RaycastHit2D hit in hits)
		{
			if (hit.collider != null)
			{
				foreach (GameObject obstacle in obstacles)
				{
					if (hit.collider.gameObject == obstacle)
					{
						return true;
					}
				}
				obstacles.Add(hit.collider.gameObject);
			}
		}
		return false;
	}

	private bool IsSuspicious(Entity e)
	{
		if (e == null) return false;

		//ignore if too far away
		if (Vector2.Distance(transform.position, e.transform.position) > suspiciousChaseRange) return false;

		//determine suspect based on entity type
		EntityType type = e.GetEntityType();
		if (type == EntityType.Shuttle) return true;
		if (type == EntityType.BotHive) return e != hive;
		if (type == EntityType.GatherBot) return !IsSibling(e);

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
		return TakeDamage(drillDmg, drillPos, destroyer, dropModifier);
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

		if (otherLayer == layerDrill)
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
		ContactPoint2D[] contacts = new ContactPoint2D[1];
		collision.GetContacts(contacts);
		Vector2 contactPoint = contacts[0].point;

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
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
		DestroySelf();
		return currentHealth <= 0f;
	}

	public void DestroySelf(bool explode)
	{
		if (explode)
		{
			//particle effects

			//sound effects

			//drop resources

		}

		hive.BotDestroyed(this);
		base.DestroySelf();
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		return speedLimit;
	}

	public override void CollectResources(ResourceDrop r)
	{
		storage.AddItem(Item.Type.Stone);
		itemsCollected++;
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
		nearbySuspects.Clear();

		if (drillableTarget != null)
		{
			drill.StopDrilling();
			anim.SetBool("Drilling", false);
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
		for (int i = 0; i < threats.Count; i++)
		{
			if (threats[i] == target)
			{
				threats.RemoveAt(i);
				if (threats.Count == 0)
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
		Rb.velocity = launchDirection;
		shakeFX.Begin(0.1f, 0f, 1f / 30f);
	}
}