using UnityEngine;
using System.Collections.Generic;

public class Shuttle : Entity
{
	#region Fields
	//singleton reference because there will only be one and many scripts may need access to this
	public static Shuttle singleton;

	[Header("Required references")]
	[Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
	public SpriteRenderer SprRend;
	[Header("Movement related")]
	[Tooltip("Rate of speed accumulation when moving forward.")]
	public float EngineStrength = 3f;
	[Tooltip("Rate of speed decay.")]
	public float Deceleration = 1f;
	[Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
	public float SpeedLimit = 3f;
	[Tooltip("When drilling, this is multiplied with the speed limit to allow for faster boost after drilling completes.")]
	public float DrillBoost = 2f;
	[Tooltip("Controls how quickly the shuttle can rotate.")]
	public float MaxRotSpeed = 10f;
	[Tooltip("Controls how effective the shuttle's deceleration mechanism is.")]
	[Range(0f, 1f)]
	public float decelerationEffectiveness = 0.01f;
	//used as a temporary storage for rigidbody velocity when the constraints are frozen
	public Vector3 _vel;
	//the rotation that the shuttle should be at
	public Vector3 _rot;
	//force of acceleration via the shuttle
	public Vector2 _accel;
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
			float rotAngle = Mathf.Deg2Rad * _rot.z;
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
	#endregion

	#region Sound Stuff
	[SerializeField]
	private AudioClip collectResourceSound;
	private float resourceCollectedTime;
	private float resourceCollectedPitch = 1f;
	private float resourceCollectedPitchIncreaseAmount = 0.2f;
	private int inventorySize = 10;
	public Inventory inventory;
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
		if (!IsDrilling && Rb.constraints != RigidbodyConstraints2D.FreezeAll)
		{
			Rb.AddForce(_accel);
		}
	}

	//Checks for input related to movement and calculates acceleration
	private void GetMovementInput()
	{
		//update rotation variable with transform's current rotation
		_rot.z = transform.eulerAngles.z;

		//get rotation input
		float lookDirection = InputHandler.GetLookDirection(transform.position);
			
		//if no rotation input has been given then use the same as last frame
		if (float.IsPositiveInfinity(lookDirection)) lookDirection = _lastLookDirection;

		//automatically look for the nearest asteroid
		if (autoPilot)
		{
			if (Time.time - autoPilotTimer > 1f || followTarget == null)
			{
				SearchForNearestAsteroid();
			}

			lookDirection = Vector2.Angle(Vector2.up, followTarget.position - transform.position);
			if (followTarget.position.x < transform.position.x)
			{
				lookDirection = 180f + (180f - lookDirection);
			}
		}
			
		//update last look direction (mostly for joystick use)
		_lastLookDirection = lookDirection;

		//determine how quickly to rotate
		//rotMod controls how smoothly the rotation happens
		float rotMod = Mathf.Abs((360f - _rot.z) - lookDirection);
		if (rotMod > 180f)
		{
			rotMod = Mathf.Abs(rotMod - 360f);
		}
		rotMod /= 180f;
		rotMod = Mathf.Pow(rotMod, 0.8f);
		SetRot(Mathf.MoveTowardsAngle(_rot.z, -lookDirection, MaxRotSpeed * rotMod));

		//reset acceleration
		_accel = Vector2.zero;
		//get movement input
		_accel.y += Mathf.Clamp01(InputHandler.GetInput("MoveVertical")) * EngineStrength;
		if (!IsDrilling)
		{
			_accel.x += InputHandler.GetInput("MoveHorizontal") * EngineStrength;
		}
		if (autoPilot)
		{
			_accel = Vector2.up * EngineStrength;
		}
		float magnitude = _accel.magnitude;

		//if no acceleration then ignore the rest
		if (Mathf.Approximately(_accel.x, 0f) && Mathf.Approximately(_accel.y, 0f)) return;
			
		//if using a joystick then don't affect direction because it doesn't feel intuitive
		if (InputHandler.GetMode() == InputHandler.InputMode.Keyboard)
		{
			//rotate forward acceleration direction to be based on the direction the shuttle is facing
			float accelAngle = Vector2.Angle(Vector2.up, _accel);
			if (_accel.x < 0)
			{
				accelAngle = 180f + (180f - accelAngle);
			}
			Vector2 shuttleDir;
			shuttleDir.x = Mathf.Sin(Mathf.Deg2Rad * (360f - _rot.z + accelAngle));
			shuttleDir.y = Mathf.Cos(Mathf.Deg2Rad * (360f - _rot.z + accelAngle));
			_accel = shuttleDir;
		}

		float topSpeed = Mathf.Min(EngineStrength, SpeedLimit);
		if (magnitude > topSpeed)
		{
			magnitude = topSpeed;
		}
		_accel *= magnitude;
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

		Vector3 addForce = _accel;

		if (IsDrilling)
		{
			//freeze constraints
			Rb.constraints = RigidbodyConstraints2D.FreezeAll;
			//add potential velocity
			_vel += addForce / 10f;
			//apply a continuous slowdown effect
			_vel = Vector3.MoveTowards(_vel, Vector3.zero, 0.1f);
			//calculate how powerful the drill can be
			float drillSpeedModifier = SpeedLimit * DrillBoost;
			drillSpeedModifier *= drillSpeedModifier;
			//set an upper limit so that the drill speed doesn't go too extreme
			if (_vel.sqrMagnitude > drillSpeedModifier)
			{
				_vel.Normalize();
				_vel *= Mathf.Sqrt(drillSpeedModifier);
			}
		}
		else
		{
			//if just recently finished drilling then this will allow the shuttle to get its velocity back instantly
			if (Rb.constraints == RigidbodyConstraints2D.FreezeAll)
			{
				Rb.velocity = _vel;
			}
			_vel = Rb.velocity;
			//keep constraints unfrozen
			Rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			Rb.drag = Mathf.MoveTowards(
				Rb.drag,
				Deceleration * decelerationModifier,
				decelerationEffectiveness);
			//set rotation
			transform.eulerAngles = _rot;
		}
	}

	public void CollectResources(ResourceDrop r)
	{
		inventory.AddItem(Item.Type.Stone);

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
		AudioManager.PlaySFX(collectResourceSound, CameraCtrl.camCtrl.transform.position, transform, pitch: resourceCollectedPitch);
		
	}

	private void SearchForNearestAsteroid()
	{
		autoPilotTimer = Time.time;

		int searchRange = 1;
		List<Entity> asteroids = EntityNetwork.GetEntitiesInRange(_coords, searchRange, EntityType.Asteroid);

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
		_rot.z = ((newRot % 360f) + 360f) % 360f;
	}
	
	public override EntityType GetEntityType() {
		return EntityType.Shuttle;
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		if (firstHit && _vel.magnitude >= SpeedLimit + 0.5f)
		{
			return InputHandler.IsHoldingBack() ? 0f : _vel.magnitude * 50f;
		}
		else
		{
			return InputHandler.IsHoldingBack() ? 0f : _vel.magnitude;
		}
	}

	public void AutoPilotSwitch(bool isOn)
	{
		autoPilot = isOn;
	}
}