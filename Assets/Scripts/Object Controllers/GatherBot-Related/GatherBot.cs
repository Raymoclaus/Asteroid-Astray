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
		Attacking
	}

	//references
	private BotHive hive;
	[SerializeField]
	private ShakeEffect shakeFX;
	[SerializeField]
	private ExpandingCircle scanningBeam;
	[SerializeField]
	private Inventory storage;

	//fields
	[SerializeField]
	private AIState state;
	private float maxHealth = 500f;
	private float currentHealth;

	//movement variables
	private Vector2 accel;
	[SerializeField]
	private float engineStrength = 1f, speedLimit = 2f, deceleration = 2f, decelEffectiveness = 0.01f,
		rotationSpeed = 3f;
	private float rot = 180f;
	private Vector2 velocity;
	private float maxSway = 45;
	private float distanceCheck = 10f;
	public Entity targetEntity;

	//scanning variables
	private float scanTimer, scanDuration = 3f;
	private bool scanStarted;
	private int entitiesScanned;

	//gathering variables
	private float searchTimer, searchInterval = 0.3f;
	private int drillCount, drillLimit = 3;
	private List<Entity> surroundingEntities;
	private IDrillableObject drillableTarget;
	[SerializeField]
	private int storageCapacity = 10;
	private int itemsCollected;

	//exploring variables
	private bool waitingForHiveDirection = true;
	public Vector2 targetLocation;

	private void Start()
	{
		transform.eulerAngles = Vector3.forward * -rot;
		initialised = true;
	}

	private void Update()
	{
		accel = Vector2.zero;

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
		Vector2 targetPos = hive.transform.position + Vector3.down * 2f;
		if (GoToLocation(targetPos))
		{
			if (Rb.velocity.sqrMagnitude < Mathf.Epsilon)
			{
				StartExploring();
				gameObject.layer = layerSolid;
				foreach (Collider2D col in Col)
				{
					col.gameObject.layer = layerSolid;
				}
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
				entitiesScanned = 0;
				new Thread(() =>
				{
					entitiesScanned = EntityNetwork.CountInRange(_coords, 1);
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

				//choose state
				if (entitiesScanned >= 10)
				{
					state = AIState.Gathering;
					canDrill = true;
				}
				else
				{
					hive.MarkCoordAsEmpty(_coords);
					state = AIState.Exploring;
					waitingForHiveDirection = true;
					hive.AssignUnoccupiedCoords(this);
				}
			}
		}
	}

	private void Gathering()
	{
		if (Time.time - searchTimer > searchInterval || targetEntity == null)
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
			if (GoToLocation(targetLocation))
			{
				state = AIState.Scanning;
			}
		}
	}

	private void Storing()
	{
		targetEntity = hive;
		if (GoToLocation(targetEntity.transform.position, true, 2f, false))
		{
			hive.Store(storage.inventory, this);
			itemsCollected = 0;
			StartExploring();
		}
	}

	private void Suspicious()
	{

	}

	private void Signalling()
	{

	}

	private void Attacking()
	{

	}

	#endregion

	private bool GoToLocation(Vector2 targetPos, bool avoidObstacles = true, float distLimit = 1f, bool adjustForMomentum = false)
	{
		float distLeft = Vector2.Distance(transform.position, targetPos);
		if (distLeft > distLimit)
		{
			float expectedAngle = DetermineDirection(targetPos, avoidObstacles);
			if (adjustForMomentum)
			{
				expectedAngle = AdjustForMomentum(expectedAngle);
			}
			float speedMod = 1f - RotateTo(expectedAngle);
			DetermineAcceleration(speedMod, distLeft, distLimit);
			return false;
		}
		return true;
	}

	public void HiveOrders(Vector2 pos)
	{
		waitingForHiveDirection = false;
		targetLocation = pos;
	}

	public ChunkCoords GetIntendedCoords()
	{
		if (state == AIState.Exploring && !waitingForHiveDirection)
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

		searchTimer = Time.time;
	}

	private float AdjustForMomentum(float lookDir)
	{
		float ld = lookDir, rt = rot;

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

		ld += difference * delta;
		while (ld < 0f)
		{
			ld += 360f;
		}

		return ld;
	}

	private void DetermineAcceleration(float speedMod, float? distLeft = null, float distLimit = 1f)
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
		accel = transform.up * Mathf.Min(mag, topAccel);
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
		rot = (Mathf.MoveTowardsAngle(rot, angle, rotationSpeed * rotMod) + 360f) % 360f;
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
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * angleTo), Mathf.Cos(Mathf.Deg2Rad * angleTo)),
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

		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit2D hit = hits[i];

			if (hit.collider != null && !IsTarget(hit))
			{
				accel *= hit.fraction;
				float delta = 1f - hit.fraction;
				if (hit.rigidbody != null)
				{
					switch (i)
					{
						case 0:
							change += maxSway * delta;
							break;
						case 1:
							Vector2 normal = hit.normal;
							float normalAngle = Vector2.Angle(Vector2.up, normal);
							bool moveRight = Mathf.MoveTowardsAngle(angleTo, normalAngle, 1f) > angleTo;
							if (moveRight)
							{
								change += maxSway * delta * 2f;
							}
							else
							{
								change -= maxSway * delta * 2f;
							}
							break;
						case 2:
							change -= maxSway * delta;
							break;
					}
				}
			}
		}

		Debug.DrawLine(transform.position, (Vector2)transform.position
			+ new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + change)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + change))), Color.red);

		return angleTo + change;
	}

	private bool IsTarget(RaycastHit2D hit)
	{
		if (targetEntity == null) return false;
		foreach (Collider2D col in targetEntity.Col)
		{
			if (hit.collider == col) return true;
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

	private IEnumerator ScanRings()
	{
		WaitForSeconds wfs = new WaitForSeconds(0.5f);
		System.Action a = () =>
		{
			ExpandingCircle scan = Instantiate(scanningBeam);
			scan.lifeTime = scanDuration;
			scan.transform.position = transform.position;
			scan.transform.parent = ParticleGenerator.singleton.transform;
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

	public void Create(BotHive botHive, float MaxHP)
	{
		hive = botHive;
		state = AIState.Spawning;
		maxHealth = MaxHP;
		currentHealth = maxHealth;
	}

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0)
	{
		return TakeDamage(drillDmg, drillPos, destroyer, dropModifier);
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		shakeFX.Begin();
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		shakeFX.Stop();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && !IsDrilling)
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this);
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
				otherDrill.StopDrilling();
			}
		}
	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0)
	{
		currentHealth -= damage;

		return CheckHealth();
	}

	public override void DrillComplete()
	{
		bool hiveOrders = hive.SplitUpGatheringUnits(this);
		drillCount++;

		if (drillCount < drillLimit && !hiveOrders) return;

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

	public override bool VerifyTarget(Entity target)
	{
		return target == targetEntity;
	}

	private bool CheckHealth()
	{
		return currentHealth <= 0f;
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
		}
	}

	private void StartExploring()
	{
		state = AIState.Exploring;
		waitingForHiveDirection = true;
		hive.AssignUnoccupiedCoords(this);
	}

	public Vector2 GetPosition()
	{
		return transform.position;
	}
}