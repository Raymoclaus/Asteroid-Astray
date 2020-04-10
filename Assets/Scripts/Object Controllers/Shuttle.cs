using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using InputHandlerSystem;
using QuestSystem;
using QuestSystem.UI;
using InventorySystem;
using InventorySystem.UI;
using ValueComponents;
using SceneControllers;
using AudioUtilities;
using DialogueSystem;
using StatisticsTracker;
using SaveSystem;

public class Shuttle : Character, IStunnable, ICombat
{
	[Header("Shuttle Fields")]
	[Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
	public SpriteRenderer SprRend;
	[Tooltip("Requires reference to the Animator of the shuttle's transform.")]
	public Animator shuttleAnimator;
	[Tooltip("Rate of speed accumulation when moving forward.")]
	public float EngineStrength = 3f;
	[Tooltip("Rate of speed decay.")] public float Deceleration = 1f;
	[Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
	public float SpeedLimit = 3f;
	[Tooltip("When drilling, this is multiplied with the speed limit to allow for faster boost after drilling completes.")]
	public float DrillBoost = 2f;
	[SerializeField] private float drillDamageMultiplier = 0.5f;
	[Tooltip("Controls how quickly the shuttle can rotate.")]
	public float MaxRotSpeed = 10f;
	[Tooltip("Controls how effective the shuttle's deceleration mechanism is.")] [Range(0f, 1f)]
	public float decelerationEffectiveness = 0.01f;
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
	//transform for the auto pilot to follow
	private Transform followTarget;
	//used to adjust speed temporarily
	private float speedMultiplier = 1f;
	//whether the shuttle can perform a drill launch
	public float drillLaunchSpeed = 10f;
	[SerializeField] private float drillLaunchMaxAngle = 60f;
	[SerializeField] private SpriteRenderer drillLaunchArcSprite;
	private bool stunned = false;
	private float stunDuration = 2f;
	[SerializeField] private float launchZoomOutSize = 5f;
	[SerializeField] private float launchLookAheadDistance = 5f;
	private List<ICombat> enemies = new List<ICombat>();
	private bool isTemporarilyInvincible = false;
	[SerializeField] private ColorReplacementGroup cRGroup;
	[SerializeField] private bool drillIsActive = true;
	[SerializeField] private bool canShoot;
	public bool CanShoot
	{
		get => canShoot && CanAttack;
		set => canShoot = value;
	}
	[SerializeField] private bool canLaunch;
	public bool CanLaunch
	{
		get => canLaunch && CanAttack;
		set => canLaunch = value;
	}
	[SerializeField] private float launchDamage = 500f;
	public bool hasControl = true;
	[SerializeField] private bool autoPilot;
	public bool isKinematic;
	[SerializeField] private TY4PlayingUI ty4pUI;
	public UnityEvent EnteringShip;
	public bool CanBoost { get; private set; } = true;
	//how long a boost can last
	[SerializeField] private RangedFloatComponent boostComponent;
	//how much boost is available as a percentage
	public float BoostPercentage => boostComponent.CurrentRatio;
	//how much a boost affects speed
	[SerializeField] private float boostSpeedMultiplier = 2f;
	//how long it takes before boost fuel begins recharging
	[SerializeField] private float boostRechargeTime = 2f;
	private string boostRechargeTimerID;
	//how quickly the boost fuel recharges
	[SerializeField] private float rechargeSpeed = 1f;
	//how much boosting ignores existing momentum
	[SerializeField] private float boostCounterVelocity = 0.1f;
	//whether the shuttle is boosting or not
	private bool IsBoosting { get; set; }
	//reference to sonic boom animation
	[SerializeField] private GameObject sonicBoomBoostEffect;
	public float boostInvulnerabilityTime = 0.2f;
	[SerializeField] private AnimationCurve cameraZoomOnEnterShip;
	private bool laserAttached = false;
	private bool straightWeaponAttached = false;
	[SerializeField] private AudioClip collectResourceSound;
	private float resourceCollectedTime;
	private float resourceCollectedPitch = 1f;
	private float resourceCollectedPitchIncreaseAmount = 0.2f;
	[SerializeField] public AudioSO collisionSounds;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	[SerializeField] private GameAction goAction,
		boostAction,
		cancelDrillingAction,
		shootAction;
	[SerializeField] private GameAction[] slotActions = new GameAction[8];
	[SerializeField] private BoolStatTracker shuttleRepairedStat, mainHatchLockedStat;
	[SerializeField] private MainHatchPrompt mainHatch;
	[SerializeField] private ConversationWithActions wormholeRecoveryConversation;

	public delegate void GoInputEventHandler();
	public event GoInputEventHandler OnGoInput;
	public delegate void LaunchInputEventHandler();
	public event LaunchInputEventHandler OnLaunchInput;
	public delegate void DrillCompleteEventHandler(bool successful);
	public event DrillCompleteEventHandler OnDrillComplete;

	protected override void Awake()
	{
		base.Awake();
		if (CameraControl) CameraControl.followTarget = this;

		canDrill = true;
		canDrillLaunch = true;
		boostComponent.SetToUpperLimit();
		boostRechargeTimerID = "Boost Recharge Timer" + gameObject.GetInstanceID();
		TimerTracker.AddTimer(boostRechargeTimerID, 0f, null, null);
		QuestPopupUI.SetQuester(GetComponent<Quester>());
		AttachToInventoryUI();

		OnItemsCollected += ReceiveItem;
		FindObjectOfType<ItemPopupUI>()?.SetInventoryHolder(this);

		//start repair shuttle questline if shuttle is damaged
		if (shuttleRepairedStat.IsFalse)
		{
			//reduce health by repair kit heal amount
			//TODO: define repair kit value somewhere
			DecreaseCurrentHealth(200f);
			//choose a random starting location nearby the main ship
			Vector2 pos = mainHatch.transform.position;
			float randomAngle = Random.value * Mathf.PI * 2f;
			Vector2 randomPos = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
			randomPos *= Random.value * 15f + 30f;
			Teleport(pos + randomPos);
			//lock main ship's hatch
			mainHatch.IsLocked = true;
			//shuttle can't attack
			CanAttack = false;
			//disable distance UI
			DistanceUI.Hidden = true;
			//start recovery dialogue
			LoadingController.AddListener(
				() => SendActiveDialogue(wormholeRecoveryConversation, true));
		}
	}

	protected override void Update()
	{
		base.Update();

		if (Input.GetKeyDown(KeyCode.U))
		{
			autoPilot = !autoPilot;
		}

		if (!stunned)
		{
			//get shuttle movement input
			GetMovementInput();
			//calculate position based on input
			CalculateForces();
		}
	}

	private void FixedUpdate() => rb.AddForce(accel);

	//Checks for input related to movement and calculates acceleration
	private void GetMovementInput()
	{
		if (!hasControl) return;

		//Check if the player is attempting to boost
		if (!autoPilot) Boost(InputManager.GetInput(boostAction) > 0f);
		//used for artificially adjusting speed, used by the auto pilot only
		float speedMod = 1f;
		//update rotation variable with transform's current rotation
		rot.z = transform.eulerAngles.z;
		//get rotation input
		float lookDirection = InputManager.GetLookAngle(transform.position);
		//if no rotation input has been given then use the same as last frame
		if (float.IsPositiveInfinity(lookDirection)) lookDirection = lastLookDirection;

		//automatically look for the nearest asteroid
		if (autoPilot)
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
				if (!IsBoosting)
				{
					Boost(speedMod > 0.9f && boostComponent.CurrentRatio >= 0.5f);
				}
				else
				{
					float boostThreshold = Mathf.MoveTowards(0f, 1f,
						                       (Vector2.Distance(transform.position, followTarget.position)) / 2f) *
					                       0.9f;
					boostThreshold = Mathf.Max(boostThreshold, 0.5f);
					Boost(speedMod > boostThreshold);
				}
			}
			else
			{
				Boost(IsBoosting);
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
		if (autoPilot)
		{
			accel = Vector2.up * EngineStrength * speedMultiplier;
		}
		else
		{
			float input = InputManager.GetInput(goAction);
			if (input > 0f)
			{
				OnGoInput?.Invoke();
			}

			if (IsDrilling)
			{
				float launchInput = InputManager.GetInput(cancelDrillingAction);
				input = Mathf.Clamp01(input + launchInput);
				if (!CanDrillLaunch&& launchInput > 0f)
				{
					input = 0f;
				}
			}

			accel.y += input * EngineStrength * speedMultiplier;
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
		rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
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

	public void ReceiveItem(ItemObject type, int amount)
	{
		if (type == ItemObject.Blank || amount == 0) return;

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
		if (AudioMngr)
		{
			AudioMngr.PlaySFX(collectResourceSound, transform.position, transform, pitch: resourceCollectedPitch);
		}
	}

	private void SearchForNearestAsteroid()
	{
		int searchRange = 1;

		float closestDistance = float.PositiveInfinity;
		Asteroid closestAsteroid = null;
		while (closestAsteroid == null)
		{
			EntityNetwork.IterateEntitiesInRange(
				coords,
				searchRange,
				e =>
				{
					if (e is Asteroid asteroid)
					{
						float dist = Vector2.Distance(transform.position, e.transform.position);
						if (dist < closestDistance)
						{
							closestDistance = dist;
							closestAsteroid = asteroid;
						}
					}

					return false;
				});
			searchRange++;
		}

		followTarget = closestAsteroid.transform;
		autoPilotTimer = Pause.timeSinceOpen;
	}

	private void SetRot(float newRot) => rot.z = ((newRot % 360f) + 360f) % 360f;

	public override EntityType GetEntityType() => EntityType.Shuttle;
	
	protected override void CheckItemUsageInput()
	{
		base.CheckItemUsageInput();

		for (int i = 0; i < 8; i++)
		{
			if (InputManager.GetInput(slotActions[i]) > 0f)
			{
				CheckItemUsage(i);
			}
		}
	}

	protected override bool CheckItemUsage(int itemIndex)
	{
		ItemObject itemType = DefaultInventory.ItemStacks[itemIndex].ItemType;
		if (!base.CheckItemUsage(itemIndex)) return false;
		return true;
	}

	protected override bool UseItem(ItemObject type)
	{
		bool used = base.UseItem(type);

		if (type.ItemName == "Repair Kit")
		{
			DistanceUI.Hidden = false;
			shuttleRepairedStat.SetValue(true);
		}

		return used;
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		if (ShouldLaunch())
		{
			CameraControl?.SetConstantSize(false);
			CameraControl?.SetLookAheadDistance(false);
			return 0f;
		}

		if (InputManager.GetInput(cancelDrillingAction) > 0f && CanDrillLaunch)
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

			CameraControl?.SetConstantSize(true, launchZoomOutSize);
			CameraControl?.SetLookAheadDistance(true, launchLookAheadDistance);
		}
		else
		{
			DrillLaunchArcDisable();
		}

		float calculation = velocity.magnitude * drillDamageMultiplier;
		if (firstHit && velocity.magnitude >= SpeedLimit + 0.5f)
		{
			return calculation * 50f;
		}
		else if (InputManager.GetInput(goAction) > 0f
		         || InputManager.GetInput(cancelDrillingAction) > 0f)
		{
			if (InputManager.GetInput(cancelDrillingAction) > 0f)
			{
				return CanDrillLaunch? 0.001f : 0f;
			}
			else
			{
				return calculation;
			}
		}
		else
		{
			return calculation;
		}
	}

	public override float MaxDrillDamage() => SpeedLimit * drillDamageMultiplier;

	public void DrillLaunchArcDisable() => drillLaunchArcSprite.gameObject.SetActive(false);

	public override void DrillComplete()
	{
		DrillLaunchArcDisable();
		CameraControl.SetConstantSize(false);
		CameraControl.SetLookAheadDistance(false);
	}

	public override bool ShouldLaunch() =>
		CanDrillLaunch && InputManager.GetInputUp(cancelDrillingAction)
		&& hasControl
		&& CanLaunch;

	public override bool CanDrillLaunch => base.CanDrillLaunch && CanLaunch;

	public void Stun()
	{
		if (isInvulnerable || isTemporarilyInvincible) return;

		drill.StopDrilling(false);
		stunned = true;
		rb.constraints = RigidbodyConstraints2D.None;
		accel = Vector2.zero;
		velocity = Vector3.zero;
		Pause.DelayedAction(() =>
		{
			stunned = false;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		}, stunDuration, true);
	}

	public override bool VerifyDrillTarget(Entity target)
	{
		if (accel == Vector2.zero) return false;
		
		if (autoPilot)
		{
			return target.GetEntityType() == EntityType.Asteroid;
		}
		return true;
	}

	public override void StoppedDrilling(bool successful)
	{
		DrillLaunchArcDisable();
		OnDrillComplete?.Invoke(successful);
		CameraControl.SetConstantSize(false);
		CameraControl.SetLookAheadDistance(false);
	}

	public override bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		float oldVal = healthComponent.CurrentRatio;
		bool dead = base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
		float newVal = healthComponent.CurrentRatio;

		if (oldVal == newVal) return false;

		if (dead)
		{
			ty4pUI?.SetActive(true);
			healthComponent.SetToUpperLimit();
			shieldValue.SetToUpperLimit();
		}
		else
		{
			cRGroup?.Flash(0.5f, Color.red);
		}
		return dead;
	}

	private void Boost(bool input)
	{
		if (CanBoost && input && boostComponent.CurrentRatio > 0f && !Pause.IsStopped)
		{
			if (!IsBoosting)
			{
				speedMultiplier *= boostSpeedMultiplier;
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
					screenRippleSO.StartRipple(this, distortionLevel: 0.02f);
					isTemporarilyInvincible = true;
					Pause.DelayedAction(() => isTemporarilyInvincible = false, boostInvulnerabilityTime, true);
				}
			}
			IsBoosting = true;
			TimerTracker.SetTimer(boostRechargeTimerID, boostRechargeTime);
			boostComponent.SubtractValue(Time.deltaTime);
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
			if (IsBoosting)
			{
				speedMultiplier /= boostSpeedMultiplier;
				shuttleAnimator.SetBool("IsBoosting", false);
			}
			IsBoosting = false;
			if (!BoostIsWaitingToRecharge)
			{
				boostComponent.AddValue(Time.deltaTime * rechargeSpeed);
			}
		}
	}

	private bool BoostIsWaitingToRecharge
		=> TimerTracker.GetTimer(boostRechargeTimerID) > 0f;

	public override void DestroyedAnEntity(Entity target)
	{
		NotifyOfDestroyedEntity(target);

		if (!CameraControl.IsInView(target.gameObject)) return;

		switch (target.GetEntityType())
		{
			case EntityType.BotHive:
			case EntityType.GatherBot:
				Pause.TemporarySlowDownEffect();
				break;
		}
	}

	public override bool CanFireLaser() =>
		laserAttached
		&& !IsBoosting
		&& InputManager.GetInput(shootAction) > 0f
		&& !Pause.IsStopped
		&& hasControl
		&& canShoot;

	public override bool CanFireStraightWeapon() =>
		straightWeaponAttached
		&& !IsBoosting
		&& InputManager.GetInput(shootAction) > 0f
		&& !Pause.IsStopped
		&& hasControl
		&& canShoot;

	public override Vector2 LaunchDirection(Transform launchableObject)
	{
		float shuttleAngle = -Vector2.SignedAngle(Vector2.up, transform.up);
		if (!hasControl)
		{
			shuttleAngle *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Sin(shuttleAngle), Mathf.Cos(shuttleAngle));
		}
		float launchAngle = InputManager.GetLookAngle(transform.position);
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
		OnLaunchInput?.Invoke();
		DrillLaunchArcDisable();
		CameraControl?.SetConstantSize(false);
		CameraControl?.SetLookAheadDistance(false);
	}

	public override float GetLaunchDamage() => launchDamage;

	public bool EngageInCombat(ICombat hostile)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] == hostile) return false;
		}
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

	public override void AttachLaser(bool attach) => laserAttached = attach;

	public override void AttachStraightWeapon(bool attach) => straightWeaponAttached = attach;

	public override ICombat GetICombat() => this;

	public override bool TakeItem(ItemObject type, int amount) => DefaultInventory.RemoveItem(type, amount);

	public override Scan ReturnScan() => new Scan(GetEntityType(), 1f, GetLevel(), GetValue());

	protected override int GetLevel() => base.GetLevel();

	protected override int GetValue() => DefaultInventory.Value;

	public override bool CanDrill()
		=> base.CanDrill()
		&& drillIsActive
		&& InputManager.GetInput(cancelDrillingAction) == 0f;

	public void EnterShip()
	{
		EnteringShip?.Invoke();
		//GameEvents.Save();

		hasControl = false;
		isKinematic = true;
		ActivateAllColliders(false);
		AnimationCurve easeInEaseOut = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		//load next scene
		SceneLoader.SceneAsync scene = SceneLoader.PrepareScene("ShipScene");
		Coroutines.TimedAction(3f, null, () => SceneLoader.LoadPreparedScene(scene));

		//move shuttle to center
		velocity = Vector3.zero;
		accel = Vector2.zero;
		rb.velocity = Vector3.zero;
		Vector3 currentPos = transform.position;
		float currentAngle = rot.z;
		Coroutines.TimedAction(2f, (float delta) =>
		{
			float evaluation = easeInEaseOut.Evaluate(delta);
			rot.z = Mathf.LerpAngle(currentAngle, 0f, evaluation);
			transform.position = Vector3.Lerp(currentPos, mainHatch.transform.position, evaluation);
			CameraControl.SetLookAheadDistance(true, 1f - delta);
		}, () =>
		{
			//zoom camera in when movement is finished
			Coroutines.TimedAction(1f, (float delta) =>
			{
				float evaluation = cameraZoomOnEnterShip.Evaluate(1f - delta);
				CameraControl.Zoom(0.5f + evaluation * 0.5f);
			}, null);
		});

		//open hatch
	}

	public override bool StartedPerformingAction(GameAction action)
		=> InputManager.GetInputDown(action);

	public override bool IsPerformingAction(GameAction action)
		=> InputManager.GetInput(action) > 0f;

	public override void Interact(object interactableObject)
	{
		if (interactableObject is MainHatchPrompt mainHatch)
		{
			EnterShip();
			return;
		}

		if (interactableObject is Planet planet)
		{
			SceneLoader.LoadScene("PlanetScene");
		}
	}

	public override bool ShouldAttack(GameAction action)
		=> base.ShouldAttack(action)
		   && IsPerformingAction(action);

	public override void ReceiveRecoil(Vector3 recoilVector)
	{
		rb.AddForce(recoilVector);
	}

	public override SaveType SaveType => SaveType.FullSave;
}