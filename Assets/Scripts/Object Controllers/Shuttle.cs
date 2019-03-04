﻿using UnityEngine;
using System.Collections.Generic;

public class Shuttle : Character, IDamageable, IStunnable, ICombat
{
	#region Fields

	[Header("Required references")]
	[SerializeField] private ShuttleTrackers trackerSO;
	[SerializeField] private CameraCtrl cameraCtrl;
	public Entity GetEntity { get { return this; } }
	[Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
	public SpriteRenderer SprRend;
	[Tooltip("Requires reference to the Animator of the shuttle's transform.")]
	public Animator shuttleAnimator;
	[Header("Movement related")]
	[Tooltip("Rate of speed accumulation when moving forward.")]
	public float EngineStrength = 3f;
	[Tooltip("Rate of speed decay.")]
	public float Deceleration = 1f;
	[Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
	public float SpeedLimit = 3f;
	[Tooltip("When drilling, this is multiplied with the speed limit to allow for faster boost after drilling" +
		" completes.")]
	public float DrillBoost = 2f;
	[SerializeField] private float drillDamageMultiplier = 0.5f;
	[Tooltip("Controls how quickly the shuttle can rotate.")]
	public float MaxRotSpeed = 10f;
	[Tooltip("Controls how effective the shuttle's deceleration mechanism is.")]
	[Range(0f, 1f)] public float decelerationEffectiveness = 0.01f;
	//used as a temporary storage for rigidbody velocity when the constraints are frozen
	public Vector3 velocity;
	//the rotation that the shuttle should be at
	public Vector3 rot;
	//force of acceleration via the shuttle
	public Vector2 accel;
	//store last look direction, useful for joysticks
	private float lastLookDirection;
	//return how far over the speed limit the shuttle's velocity is
	private float SpeedCheck
	{
		get
		{
			Vector2 vel = rb.velocity;
			float sqrMag = vel.sqrMagnitude;
			float spdLimit = SpeedLimit * SpeedLimit;

			//formula for ellipsoid, determines if velocity is within range
			//for reference: https://www.maa.org/external_archive/joma/Volume8/Kalman/General.html
			//slightly modified for use with square magnitude for better efficiency
			//half and full would normally be squared
			float rotAngle = Mathf.Deg2Rad * rot.z;
			float a = vel.x * Mathf.Cos(rotAngle) + vel.y * Mathf.Sin(rotAngle);
			float b = vel.x * Mathf.Sin(rotAngle) - vel.y * Mathf.Cos(rotAngle);
			//speed limit is halved for sideways movement
			float sidewaysLimit = spdLimit / 4f;
			float forwardLimit = spdLimit;
			a *= a;
			b *= b;
			float speedCheck = (a / sidewaysLimit) + (b / forwardLimit);
			return speedCheck;
		}
	}
	//efficiency with the searching algorithm used by the auto pilot
	private float autoPilotTimer;
	private List<Entity> asteroids = new List<Entity>();
	//transform for the auto pilot to follow
	private Transform followTarget;
	//used to adjust speed temporarily
	private float speedMultiplier = 1f;
	//whether the shuttle can perform a drill launch
	public float drillLaunchSpeed = 10f;
	[SerializeField] private float drillLaunchMaxAngle = 60f;
	[SerializeField] private SpriteRenderer drillLaunchArcSprite;
	[SerializeField] private GameObject drillLaunchImpact;
	[SerializeField] private LaunchTrailController launchTrail;
	private bool stunned = false;
	private float stunDuration = 2f;
	[SerializeField] private float launchZoomOutSize = 5f;
	[SerializeField] private float launchLookAheadDistance = 5f;
	private List<ICombat> enemies = new List<ICombat>();
	public Inventory storage;
	[SerializeField] private ShipInventory shipStorage;
	[SerializeField] private ItemPopupUI popupUI;
	private bool isInvulnerable = false;
	private QuestLog questLog = new QuestLog();
	[SerializeField] private ColorReplacementGroup cRGroup;

	#region Boost
	//whether boost capability is available
	[SerializeField] private readonly bool boostAvailable = true;
	//how long a boost can last
	[SerializeField] private float boostCapacity = 1f;
	//represents how much boost is currently available
	private float boostLevel;
	//how much a boost affects speed
	[SerializeField] private float boostMultiplier = 2f;
	//how long it takes before boost fuel begins recharging
	[SerializeField] private float boostRechargeTime = 2f;
	private float rechargeTimer;
	//how quickly the boost fuel recharges
	[SerializeField] private float rechargeSpeed = 1f;
	//how much boosting ignores existing momentum
	[SerializeField] private float boostCounterVelocity = 0.1f;
	//whether the shuttle is boosting or not
	private bool isBoosting = false;
	//reference to sonic boom animation
	[SerializeField] private GameObject sonicBoomBoostEffect;
	public float boostInvulnerabilityTime = 0.2f;
	#endregion Boost

	#endregion Fields

	#region Attachments
	private bool laserAttached = false;
	private bool straightWeaponAttached = false;
	#endregion

	#region Sound Stuff
	[SerializeField] private AudioClip collectResourceSound;
	private float resourceCollectedTime;
	private float resourceCollectedPitch = 1f;
	private float resourceCollectedPitchIncreaseAmount = 0.2f;
	[SerializeField] public AudioSO collisionSounds;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	#endregion

	public override void Awake()
	{
		base.Awake();
		shipStorage = shipStorage ?? FindObjectOfType<ShipInventory>();
		cameraCtrl = cameraCtrl ?? Camera.main.GetComponent<CameraCtrl>();
		if (cameraCtrl) cameraCtrl.followTarget = this;
	}

	private void Update()
	{
		if (!stunned)
		{
			//get shuttle movement input
			GetMovementInput();
			//calculate position based on input
			CalculateForces();
		}

		UpdateShuttleTrackerSO();
	}

	private void FixedUpdate()
	{
		rb.AddForce(accel);
	}

	//Checks for input related to movement and calculates acceleration
	private void GetMovementInput()
	{
		if (!trackerSO.hasControl) return;

		//Check if the player is attempting to boost
		if (!trackerSO.autoPilot) Boost(InputHandler.GetInput(InputHandler.InputAction.Boost) > 0f);
		//used for artificially adjusting speed, used by the auto pilot only
		float speedMod = 1f;
		//update rotation variable with transform's current rotation
		rot.z = transform.eulerAngles.z;
		//get rotation input
		float lookDirection = InputHandler.GetLookDirection(transform.position);
		//if no rotation input has been given then use the same as last frame
		if (float.IsPositiveInfinity(lookDirection)) lookDirection = lastLookDirection;

		//automatically look for the nearest asteroid
		if (trackerSO.autoPilot)
		{
			if (Pause.timeSinceOpen - autoPilotTimer > 0f || followTarget == null)
			{
				SearchForNearestAsteroid();
			}

			lookDirection = -Vector2.SignedAngle(Vector2.up, followTarget.position - transform.position);

			lookDirection = AdjustForMomentum(lookDirection);
			if (!IsDrilling)
			{
				speedMod *= 1f - Mathf.Abs(lookDirection - (360f - rot.z)) / 180f;
				if (!isBoosting)
				{
					Boost(speedMod > 0.9f && GetBoostRemaining() > 0.5f);
				}
				else
				{
					float boostThreshold = Mathf.MoveTowards(0f, 1f,
						(Vector2.Distance(transform.position, followTarget.position)) / 2f) * 0.9f;
					boostThreshold = Mathf.Max(boostThreshold, 0.5f);
					Boost(speedMod > boostThreshold);
				}
			}
			else
			{
				Boost(isBoosting);
			}
		}

		//update last look direction (mostly for joystick use)
		lastLookDirection = lookDirection;

		//determine how quickly to rotate
		//rotMod controls how smoothly the rotation happens
		float rotMod = Mathf.Abs((360f - rot.z) - lookDirection);
		if (rotMod > 180f)
		{
			rotMod = Mathf.Abs(rotMod - 360f);
		}
		rotMod /= 180f;
		rotMod = Mathf.Pow(rotMod, 0.8f);
		rot.y = Mathf.Lerp(rot.y, rotMod * 45f, Time.deltaTime * 60f);
		SetRot(Mathf.MoveTowardsAngle(rot.z, -lookDirection, MaxRotSpeed * rotMod * Time.deltaTime * 60f));

		//reset acceleration
		accel = Vector2.zero;
		//get movement input
		accel.y += Mathf.Clamp01(InputHandler.GetInput(InputHandler.InputAction.Go)) * EngineStrength * speedMultiplier;

		if (trackerSO.autoPilot)
		{
			accel = Vector2.up * EngineStrength * speedMultiplier;
		}
		float magnitude = accel.magnitude;

		//if no acceleration then ignore the rest
		if (Mathf.Approximately(accel.x, 0f) && Mathf.Approximately(accel.y, 0f)) return;

		//rotate forward acceleration direction to be based on the direction the shuttle is facing
		float accelAngle = -Vector2.SignedAngle(Vector2.up, accel);
		Vector2 shuttleDir;
		shuttleDir.x = Mathf.Sin(Mathf.Deg2Rad * (360f - rot.z + accelAngle));
		shuttleDir.y = Mathf.Cos(Mathf.Deg2Rad * (360f - rot.z + accelAngle));
		accel = shuttleDir;

		float topSpeed = Mathf.Min(EngineStrength, SpeedLimit) * speedMultiplier;
		if (magnitude > topSpeed)
		{
			magnitude = topSpeed;
		}
		accel *= magnitude * speedMod;
	}

	//use calculated rotation and speed to determine where to move to
	private void CalculateForces()
	{
		rb.bodyType = trackerSO.isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
		//calculate drag factor
		float checkSpeed = SpeedCheck;
		float decelerationModifier = 1f;
		if (checkSpeed > 1f)
		{
			decelerationModifier *= checkSpeed;
		}

		Vector3 addForce = accel;

		if (IsDrilling)
		{
			//freeze constraints
			rb.constraints = RigidbodyConstraints2D.FreezeAll;
			//add potential velocity
			velocity += addForce / 10f;
			//apply a continuous slowdown effect
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, 0.1f);
			//calculate how powerful the drill can be
			float drillSpeedModifier = SpeedLimit * speedMultiplier * DrillBoost;
			drillSpeedModifier *= drillSpeedModifier;
			//set an upper limit so that the drill speed doesn't go too extreme
			if (velocity.sqrMagnitude > drillSpeedModifier)
			{
				velocity.Normalize();
				velocity *= Mathf.Sqrt(drillSpeedModifier);
			}
		}
		else
		{
			//if just recently finished drilling then this will allow the shuttle to get its velocity back instantly
			if (rb.constraints == RigidbodyConstraints2D.FreezeAll)
			{
				rb.velocity = velocity;
			}
			velocity = rb.velocity;
			//keep constraints unfrozen
			rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			rb.drag = Mathf.MoveTowards(
				rb.drag,
				Deceleration * decelerationModifier,
				decelerationEffectiveness);
			//set rotation
			transform.eulerAngles = rot;
		}
	}

	private float AdjustForMomentum(float lookDir)
	{
		float ld = lookDir, rt = 360f - rot.z;

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

	public override void CollectResources(Item.Type type, int amount)
	{
		int collectedAmount = amount - storage.AddItem(type, num: amount);
		if (collectedAmount > 0)
		{
			GameEvents.ItemCollected(type, collectedAmount);
		}

		//increase pitch of sound for successive resource collection, reset after a break
		if (Pause.timeSinceOpen - resourceCollectedTime < 1f)
		{
			resourceCollectedPitch += resourceCollectedPitchIncreaseAmount;
		}
		else
		{
			resourceCollectedPitch = 1f;
		}
		resourceCollectedTime = Pause.timeSinceOpen;
		//play resource collect sound
		audioManager = audioManager ?? FindObjectOfType<AudioManager>();
		if (audioManager)
		{
			audioManager.PlaySFX(collectResourceSound, transform.position, transform, pitch: resourceCollectedPitch);
		}
		popupUI = popupUI ?? FindObjectOfType<ItemPopupUI>();
		if (popupUI)
		{
			popupUI.GeneratePopup(type, amount);
		}

	}

	private void SearchForNearestAsteroid()
	{
		autoPilotTimer = Pause.timeSinceOpen;

		int searchRange = 1;
		asteroids.Clear();

		while (asteroids.Count == 0)
		{
			EntityNetwork.GetEntitiesInRange(_coords, searchRange, EntityType.Asteroid, addToList: asteroids);
			searchRange++;
		}

		float shortestDist = float.PositiveInfinity;
		for (int i = 0; i < asteroids.Count; i++)
		{
			Entity e = asteroids[i];
			float dist = Vector2.Distance(transform.position, e.transform.position);
			if (dist < shortestDist || float.IsPositiveInfinity(shortestDist))
			{
				shortestDist = dist;
				followTarget = e.transform;
			}
		}
	}

	private void SetRot(float newRot)
	{
		rot.z = ((newRot % 360f) + 360f) % 360f;
	}

	public override EntityType GetEntityType() {
		return EntityType.Shuttle;
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		if (InputHandler.GetInputUp(InputHandler.InputAction.Launch) > 0f || !trackerSO.hasControl)
		{
			if (cameraCtrl)
			{
				cameraCtrl.SetConstantSize(false);
				cameraCtrl.SetLookAheadDistance(false);
			}
			return 0f;
		}

		if (InputHandler.GetInput(InputHandler.InputAction.Launch) > 0f)
		{
			GameObject launchCone = drillLaunchArcSprite.gameObject;
			launchCone.SetActive(true);
			launchCone.transform.position = ((Entity)(drill.drillTarget)).transform.position;
			launchCone.transform.eulerAngles = Vector3.forward * transform.eulerAngles.z;
			drillLaunchArcSprite.material.SetFloat("_ArcAngle", drillLaunchMaxAngle);
			Transform arrow = launchCone.transform.GetChild(0);
			Vector2 launchDir = LaunchDirection(((Entity)(drill.drillTarget)).transform);
			float angle = Vector2.SignedAngle(Vector2.up, launchDir);
			arrow.eulerAngles = Vector3.forward * angle;
			arrow.position = ((Entity)(drill.drillTarget)).transform.position;
			if (cameraCtrl)
			{
				cameraCtrl.SetConstantSize(true, launchZoomOutSize);
				cameraCtrl.SetLookAheadDistance(true, launchLookAheadDistance);
			}
		}
		else
		{
			DrillLaunchArcDisable();
		}

		if (firstHit && velocity.magnitude >= SpeedLimit + 0.5f)
		{
			return velocity.magnitude * 50f * drillDamageMultiplier;
		}
		else
		{
			return velocity.magnitude * drillDamageMultiplier;
		}
	}

	public override float MaxDrillDamage()
	{
		return SpeedLimit * drillDamageMultiplier;
	}

	public void DrillLaunchArcDisable()
	{
		drillLaunchArcSprite.gameObject.SetActive(false);
	}

	public override void DrillComplete()
	{
		DrillLaunchArcDisable();
		cameraCtrl.SetConstantSize(false);
		cameraCtrl.SetLookAheadDistance(false);
	}

	public override bool ShouldLaunch()
	{
		return canDrillLaunch
			&& velocity.sqrMagnitude >= Mathf.Pow(SpeedLimit * DrillBoost, 2f) * 0.9f
			&& InputHandler.GetInputUp(InputHandler.InputAction.Launch) > 0f
			&& trackerSO.hasControl;
	}

	public void Stun()
	{
		if (isInvulnerable) return;

		drill.StopDrilling();
		stunned = true;
		rb.constraints = RigidbodyConstraints2D.None;
		accel = Vector2.zero;
		velocity = Vector3.zero;
		pause = pause ?? FindObjectOfType<Pause>();
		if (pause)
		{
			pause.DelayedAction(() =>
			{
				stunned = false;
				rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			}, stunDuration, true);
		}
	}

	public void StoreInShip()
	{
		shipStorage = shipStorage ?? FindObjectOfType<ShipInventory>();
		if (shipStorage) shipStorage.Store(storage.stacks);
	}

	public override bool VerifyDrillTarget(Entity target)
	{
		bool checks = true;
		if (trackerSO.autoPilot)
		{
			checks = target.GetEntityType() == EntityType.Asteroid;
		}
		else
		{
			checks = accel != Vector2.zero;
			if (target.GetEntityType() != EntityType.Asteroid)
			{
				checks = checks && enemies.Count > 0;
			}
		}
		return checks;
	}

	public override void StoppedDrilling()
	{
		DrillLaunchArcDisable();
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

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
		}
	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0, bool flash = true)
	{
		if (destroyer == this || isInvulnerable) return false;
		
		cRGroup?.Flash(0.5f, Color.red);

		return true;
	}

	public Vector2 GetPosition()
	{
		return transform.position;
	}

	private void Boost(bool input)
	{
		if (boostAvailable && input && boostLevel < boostCapacity && !Pause.IsPaused)
		{
			if (!isBoosting)
			{
				speedMultiplier *= boostMultiplier;
				if (sonicBoomBoostEffect != null && !IsDrilling)
				{
					rb.velocity = transform.up;
					shuttleAnimator.SetBool("IsBoosting", true);
					Transform effect = Instantiate(sonicBoomBoostEffect).transform;
					effect.parent = ParticleGenerator.holder;
					effect.position = transform.position;
					Vector3 effectRotation = effect.eulerAngles;
					effectRotation += transform.eulerAngles;
					effect.eulerAngles = effectRotation;
					screenRippleSO.StartRipple(this, distortionLevel: 0.01f);
					isInvulnerable = true;
					pause = pause ?? FindObjectOfType<Pause>();
					pause.DelayedAction(() => isInvulnerable = false, boostInvulnerabilityTime, true);
				}
			}
			isBoosting = true;
			rechargeTimer = 0f;
			boostLevel += Time.deltaTime;
			if (IsDrilling)
			{
				velocity.Normalize();
				velocity *= SpeedLimit * speedMultiplier * DrillBoost;
			}
			else
			{
				Vector3 rbVel = rb.velocity;
				rbVel = Vector3.Lerp(rbVel, transform.up, boostCounterVelocity);
				rbVel.Normalize();
				rbVel *= SpeedLimit * speedMultiplier;
				rb.velocity = rbVel;
			}
		}
		else
		{
			if (isBoosting)
			{
				speedMultiplier /= boostMultiplier;
				shuttleAnimator.SetBool("IsBoosting", false);
			}
			isBoosting = false;
			rechargeTimer += Time.deltaTime;
			if (rechargeTimer >= boostRechargeTime)
			{
				boostLevel = Mathf.Max(boostLevel - Time.deltaTime * rechargeSpeed, 0f);
			}
		}
	}

	public override void DestroyedAnEntity(Entity target)
	{
		GameEvents.EntityDestroyed(target.GetEntityType());

		switch (target.GetEntityType())
		{
			case EntityType.BotHive:
			case EntityType.GatherBot:
			{
				pause = pause ?? FindObjectOfType<Pause>();
				if (pause)
				{
					pause.TemporarySlowDownEffect();
				}
			break;
			}
		}
	}

	public float GetBoostRemaining()
	{
		return (boostCapacity - boostLevel) / boostCapacity;
	}

	public override bool CanFireLaser()
	{
		return laserAttached && !isBoosting && InputHandler.GetInput(InputHandler.InputAction.Shoot) > 0f
			&& !Pause.IsPaused && trackerSO.hasControl;
	}

	public override bool CanFireStraightWeapon()
	{
		return straightWeaponAttached && !isBoosting && InputHandler.GetInput(InputHandler.InputAction.Shoot) > 0f
			&& !Pause.IsPaused && trackerSO.hasControl;
	}

	public override GameObject GetLaunchImpactAnimation()
	{
		return drillLaunchImpact;
	}

	public override LaunchTrailController GetLaunchTrailAnimation()
	{
		return launchTrail;
	}

	public override Vector2 LaunchDirection(Transform launchableObject)
	{
		float shuttleAngle = -Vector2.SignedAngle(Vector2.up, transform.up);
		if (!trackerSO.hasControl)
		{
			shuttleAngle *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Sin(shuttleAngle), Mathf.Cos(shuttleAngle));
		}
		float launchAngle = InputHandler.GetLookDirection(transform.position);
		if (float.IsPositiveInfinity(launchAngle)) launchAngle = lastLookDirection;
		float launchAngleRad = launchAngle * Mathf.Deg2Rad;
		Vector2 launchDir = new Vector2(Mathf.Sin(launchAngleRad), Mathf.Cos(launchAngleRad));
		float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(launchAngle, shuttleAngle));
		if (deltaAngle > drillLaunchMaxAngle / 2f)
		{
			launchAngle = Mathf.MoveTowardsAngle(shuttleAngle, launchAngle, drillLaunchMaxAngle / 2f);
			launchAngle *= Mathf.Deg2Rad;
			launchDir = new Vector2(Mathf.Sin(launchAngle), Mathf.Cos(launchAngle));
		}
		launchDir *= drillLaunchSpeed;
		return launchDir;
	}

	public override void Launching()
	{
		DrillLaunchArcDisable();
	}

	public override float GetLaunchDamage()
	{
		return trackerSO.launchDamage;
	}

	public bool EngageInCombat(ICombat hostile)
	{
		if (enemies.Contains(hostile)) return false;
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

	private void UpdateShuttleTrackerSO()
	{
		if (!trackerSO)
		{
			print("Attach appropriate Scriptable Object tracker to " + GetType().Name);
			return;
		}

		trackerSO.position = transform.position;
		trackerSO.rotation = transform.eulerAngles;
		trackerSO.velocity = velocity;
		trackerSO.lastLookDirection = lastLookDirection;
		trackerSO.boostRemaining = GetBoostRemaining();
		trackerSO.storageCount = storage.Count(Item.Type.Stone);
	}

	public override void AttachLaser(bool attach)
	{
		laserAttached = attach;
	}

	public override void AttachStraightWeapon(bool attach)
	{
		straightWeaponAttached = attach;
	}

	public override ICombat GetICombat()
	{
		return this;
	}

	public override void ReceiveItemReward(Item.Type type, int amount)
	{
		CollectResources(type, amount);
	}

	public override void AcceptQuest(Quest quest)
	{
		base.AcceptQuest(quest);

		Debug.Log($"Accepted Quest: {quest.Name}");
		for (int i = 0; i < quest.Requirements.Count; i++)
		{
			QuestRequirement req = quest.Requirements[i];
			Debug.Log(req.GetDescription());
		}
		questLog.AddQuest(quest);
	}
}