using UnityEngine;
using System.Collections.Generic;

public class Shuttle : Entity, IDamageable
{
	#region Fields
	//singleton reference because there will only be one and many scripts may need access to this
	public static Shuttle singleton;

	[Header("Required references")]
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
	[SerializeField]
	private float drillDamageMultiplier = 0.5f;
	[Tooltip("Controls how quickly the shuttle can rotate.")]
	public float MaxRotSpeed = 10f;
	[Tooltip("Controls how effective the shuttle's deceleration mechanism is.")]
	[Range(0f, 1f)]
	public float decelerationEffectiveness = 0.01f;
	//used as a temporary storage for rigidbody velocity when the constraints are frozen
	public Vector3 velocity;
	//the rotation that the shuttle should be at
	public Vector3 rot;
	//force of acceleration via the shuttle
	public Vector2 accel;
	//store last look direction, useful for joysticks
	private float _lastLookDirection;
	//return how far over the speed limit the shuttle's velocity is
	private float SpeedCheck
	{
		get
		{
			Vector2 vel = Rb.velocity;
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
	//automatically move towards nearby asteroids and drill them
	private bool autoPilot;
	//efficiency with the searching algorithm used by the auto pilot
	private float autoPilotTimer;
	//transform for the auto pilot to follow
	private Transform followTarget;
	//used to adjust speed temporarily
	private float speedMultiplier = 1f;
	#region Boost
	//whether boost capability is available
	[SerializeField]
	private bool boostAvailable = true;
	//how long a boost can last
	[SerializeField]
	private float boostCapacity = 1f;
	//represents how much boost is currently available
	private float boostLevel;
	//how much a boost affects speed
	[SerializeField]
	private float boostMultiplier = 2f;
	//how long it takes before boost fuel begins recharging
	[SerializeField]
	private float boostRechargeTime = 2f;
	private float rechargeTimer;
	//how quickly the boost fuel recharges
	[SerializeField]
	private float rechargeSpeed = 1f;
	//how much boosting ignores existing momentum
	[SerializeField]
	private float boostCounterVelocity = 0.1f;
	//whether the shuttle is boosting or not
	private bool isBoosting = false;
	//reference to sonic boom animation
	[SerializeField]
	private GameObject sonicBoomBoostEffect;
	#endregion Boost
	#endregion Fields

	#region Attachments
	// Laser Weapon
	//[SerializeField]
	//private bool laserAttached = false;
	//[SerializeField]
	//private GameObject laserWeaponObj;
	#endregion

	#region Sound Stuff
	[SerializeField]
	private AudioClip collectResourceSound;
	private float resourceCollectedTime;
	private float resourceCollectedPitch = 1f;
	private float resourceCollectedPitchIncreaseAmount = 0.2f;
	public Inventory storage;
	#endregion

	public override void Awake()
	{
		base.Awake();
		singleton = this;
	}

	private void Update()
	{
		//get shuttle movement input
		GetMovementInput();
		//calculate position based on input
		CalculateForces();
	}

	private void FixedUpdate()
	{
		Rb.AddForce(accel);
	}

	//Checks for input related to movement and calculates acceleration
	private void GetMovementInput()
	{
		//Check if the player is attempting to boost
		if (!autoPilot) Boost(Input.GetKey(KeyCode.Space));
		//used for artificially adjusting speed, used by the auto pilot only
		float speedMod = 1f;
		//update rotation variable with transform's current rotation
		rot.z = transform.eulerAngles.z;
		//get rotation input
		float lookDirection = InputHandler.GetLookDirection(transform.position);
		//if no rotation input has been given then use the same as last frame
		if (float.IsPositiveInfinity(lookDirection)) lookDirection = _lastLookDirection;

		//automatically look for the nearest asteroid
		if (autoPilot)
		{
			if (Time.time - autoPilotTimer > 0f || followTarget == null)
			{
				SearchForNearestAsteroid();
			}

			lookDirection = Vector2.Angle(Vector2.up, followTarget.position - transform.position);
			if (followTarget.position.x < transform.position.x)
			{
				lookDirection = 180f + (180f - lookDirection);
			}

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
		_lastLookDirection = lookDirection;

		//determine how quickly to rotate
		//rotMod controls how smoothly the rotation happens
		float rotMod = Mathf.Abs((360f - rot.z) - lookDirection);
		if (rotMod > 180f)
		{
			rotMod = Mathf.Abs(rotMod - 360f);
		}
		rotMod /= 180f;
		rotMod = Mathf.Pow(rotMod, 0.8f);
		SetRot(Mathf.MoveTowardsAngle(rot.z, -lookDirection, MaxRotSpeed * rotMod * Time.deltaTime * 60f));

		//reset acceleration
		accel = Vector2.zero;
		//get movement input
		accel.y += Mathf.Clamp01(InputHandler.GetInput("MoveVertical")) * EngineStrength * speedMultiplier;

		if (autoPilot)
		{
			accel = Vector2.up * EngineStrength * speedMultiplier;
		}
		float magnitude = accel.magnitude;

		//if no acceleration then ignore the rest
		if (Mathf.Approximately(accel.x, 0f) && Mathf.Approximately(accel.y, 0f)) return;
			
		//if using a joystick then don't affect direction because it doesn't feel intuitive
		if (InputHandler.GetMode() == InputHandler.InputMode.Keyboard)
		{
			//rotate forward acceleration direction to be based on the direction the shuttle is facing
			float accelAngle = Vector2.Angle(Vector2.up, accel);
			if (accel.x < 0)
			{
				accelAngle = 180f + (180f - accelAngle);
			}
			Vector2 shuttleDir;
			shuttleDir.x = Mathf.Sin(Mathf.Deg2Rad * (360f - rot.z + accelAngle));
			shuttleDir.y = Mathf.Cos(Mathf.Deg2Rad * (360f - rot.z + accelAngle));
			accel = shuttleDir;
		}

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
			Rb.constraints = RigidbodyConstraints2D.FreezeAll;
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
			if (Rb.constraints == RigidbodyConstraints2D.FreezeAll)
			{
				Rb.velocity = velocity;
			}
			velocity = Rb.velocity;
			//keep constraints unfrozen
			Rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			Rb.drag = Mathf.MoveTowards(
				Rb.drag,
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

	public override void CollectResources(ResourceDrop r)
	{
		storage.AddItem(Item.Type.Stone);

		//increase pitch of sound for successive resource collection, reset after a break
		if (Time.time - resourceCollectedTime < 1f)
		{
			resourceCollectedPitch += resourceCollectedPitchIncreaseAmount;
		}
		else
		{
			resourceCollectedPitch = 1f;
		}
		resourceCollectedTime = Time.time;
		//play resource collect sound
		AudioManager.PlaySFX(collectResourceSound, transform.position, transform, pitch: resourceCollectedPitch);
		
	}

	private void SearchForNearestAsteroid()
	{
		autoPilotTimer = Time.time;

		int searchRange = 1;
		List<Entity> asteroids = new List<Entity>();

		while (asteroids.Count == 0)
		{
			asteroids = EntityNetwork.GetEntitiesInRange(_coords, searchRange, EntityType.Asteroid);
			searchRange++;
		}

		float shortestDist = float.PositiveInfinity;
		foreach (Entity e in asteroids)
		{
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
		if (InputHandler.IsHoldingBack()) return 0f;

		if (firstHit && velocity.magnitude >= SpeedLimit + 0.5f)
		{
			return velocity.magnitude * 50f * drillDamageMultiplier;
		}
		else
		{
			return velocity.magnitude * drillDamageMultiplier;
		}
	}

	public void AutoPilotSwitch(bool isOn)
	{
		autoPilot = isOn;
	}

	public void StoreInShip()
	{
		ShipInventory.Store(storage.inventory);
	}

	public override bool VerifyDrillTarget(Entity target)
	{
		return accel != Vector2.zero;
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

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0)
	{
		if (destroyer != this)
		{
			//take damage
		}
		return false;
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
					Rb.velocity = transform.up;
					shuttleAnimator.SetBool("IsBoosting", true);
					Transform effect = Instantiate(sonicBoomBoostEffect).transform;
					effect.parent = ParticleGenerator.singleton.transform;
					effect.position = transform.position;
					Vector3 effectRotation = effect.eulerAngles;
					effectRotation += transform.eulerAngles;
					effect.eulerAngles = effectRotation;
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
				Vector3 rbVel = Rb.velocity;
				rbVel = Vector3.Lerp(rbVel, transform.up, boostCounterVelocity);
				rbVel.Normalize();
				rbVel *= SpeedLimit * speedMultiplier;
				Rb.velocity = rbVel;
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

	public float GetBoostRemaining()
	{
		return (boostCapacity - boostLevel) / boostCapacity;
	}

	public override bool CanFireLaser()
	{
		return !isBoosting;
	}

	#region Attach/Detach Methods

	//public void AttachDetachLaser1(bool attach)
	//{
	//	laserObj.SetActive(attach);
	//	laserAttached = attach;
	//}

	#endregion
}