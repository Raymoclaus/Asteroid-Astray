using System.Collections.Generic;
using AttackData;
using UnityEngine;

[RequireComponent(typeof(IPhysicsController))]
public abstract class PlanetRoomEntity : PlanetRoomObject, IAttackReceiver
{
	private IPhysicsController physicsController;
	protected IPhysicsController PhysicsController
		=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());

	[Header("Generic Entity Fields")]
	[SerializeField] private CharacterAnimationController cac;
	[SerializeField] protected Transform pivot;
	private Vector3 currentPosition;
	protected static RoomViewer roomViewer;
	[SerializeField] private Collider2D hitbox;
	[SerializeField] private float maxHealth = 100f;
	public float Health { get; private set; }
	public delegate void HealthChangedEventHandler(float previousValue, float currentValue);
	public event HealthChangedEventHandler OnHealthChanged;
	[SerializeField] private List<Loot> loot;
	private string iFrameTimerID;

	private static int entityLayer = -1;
	protected static int EntityLayer => entityLayer == -1 ?
		entityLayer = LayerMask.NameToLayer("Entity")
		: entityLayer;

	protected bool IsStunned => TimerTracker.GetTimer(stunTimerID) > 0f;
	private string stunTimerID;

	[Header("Item Usage Fields")]
	public Inventory storage;
	public delegate void ItemUsedEventHandler(Item.Type type);
	public event ItemUsedEventHandler OnItemUsed;
	[SerializeField] private string itemUseCooldownTimerID;
	[SerializeField] private float itemUseCooldownDuration = 1f;
	[SerializeField] private float itemUseRecoveryDuration = 0.5f;

	[Header("Rolling Fields")]
	[SerializeField] private float rollIFrameDuration = 0.4f;
	[SerializeField] private float rollSpeed = 0.3f;
	[SerializeField] private float rollDuration = 1f;
	private string rollTimerID;
	[SerializeField] private float rollSmoothness = 1f;
	private Vector3 RollDirection { get; set; }

	[Header("Blocking Fields")]
	[SerializeField] private float blockMaxAngle = 90f;
	[SerializeField] private float blockDamageReduction = 1f;
	private bool IsBlocking { get; set; }

	protected string actionRecoveryTimerID;

	protected virtual void Awake()
	{
		stunTimerID = gameObject.GetInstanceID() + "Stun Timer";
		TimerTracker.AddTimer(stunTimerID, 0f, null, null);
		iFrameTimerID = gameObject.GetInstanceID() + "IFrame Timer";
		TimerTracker.AddTimer(iFrameTimerID, 0f, () => EnableHitbox = true, null);
		itemUseCooldownTimerID = gameObject.GetInstanceID() + "Item Use Cooldown Timer";
		TimerTracker.AddTimer(itemUseCooldownTimerID, 0f, null, null);
		actionRecoveryTimerID = gameObject.GetInstanceID() + "Action Recovery Timer";
		TimerTracker.AddTimer(actionRecoveryTimerID, 0f, null, null);
		rollTimerID = gameObject.GetInstanceID() + "Roll Timer";
		TimerTracker.AddTimer(rollTimerID, 0f, null, null);

		roomViewer = roomViewer ?? FindObjectOfType<RoomViewer>();

		SetHealth(maxHealth);
		OnHealthChanged += HealthChanged;
	}

	protected virtual void Update()
	{
		if (pivot.position != currentPosition)
		{
			UpdateRoomObjectPosition(pivot.position);
		}

		if (CanUseItem)
		{
			CheckItemUsageInput();
		}
	}

	protected void FixedUpdate()
	{
		if (IsRolling)
		{
			float delta = RollTimer / rollDuration;
			delta = Mathf.Pow(delta, rollSmoothness);
			PhysicsController.SetVelocity(RollDirection * rollSpeed * delta);
		}
	}

	private void UpdateRoomObjectPosition(Vector2 position)
	{
		currentPosition = pivot.position;
		IntPair roundedPosition = new IntPair(Mathf.FloorToInt(position.x),
			Mathf.FloorToInt(position.y));
		SetRoomObjectWorldSpacePosition(roundedPosition);
	}

	protected void SetHealth(float healthAmount)
	{
		Health = healthAmount;
		Debug.Log($"{gameObject.name}'s health is now {Health}");
	}

	protected void AddHealth(float healthAmount)
	{
		float originalHealth = Health;
		SetHealth(Health + healthAmount);
		OnHealthChanged?.Invoke(originalHealth, Health);
	}

	protected virtual void DestroySelf()
	{
		room.RemoveObject(roomObject);
		DropLoot();
		Destroy(gameObject);
	}

	protected virtual void DropLoot() { }

	public Transform Pivot => pivot ?? transform;

	public Vector3 GetPivotPosition() => Pivot.position;

	protected bool RecoveringFromAction => TimerTracker.GetTimer(actionRecoveryTimerID) > 0f;

	protected bool EnableHitbox
	{
		get { return hitbox.enabled; }
		set { if (hitbox != null) hitbox.enabled = value; }
	}

	protected void DeactivateHitboxForDuration(float duration)
	{
		EnableHitbox = false;
		TimerTracker.SetTimer(iFrameTimerID, duration);
	}

	protected virtual void TakeHit(AttackManager atkM)
	{
		if (atkM == null) return;
		if (!CanTakeHit) return;
		if (!VerifyAttack(atkM)) return;

		bool blocked = CanBlockAttack(atkM);

		TakeKnockbackHit(atkM.GetData<KnockbackComponent>(), blocked);
		TakeStunHit(atkM.GetData<StunComponent>(), blocked);
		TakeDamage(atkM.GetData<DamageComponent>(), blocked);

		atkM.Hit(this);
	}

	protected virtual bool CanTakeHit => EnableHitbox;

	protected virtual bool VerifyAttack(AttackManager atkM)
		=> (MonoBehaviour)atkM.GetData<OwnerComponent>() != this;

	private bool CanBlockAttack(AttackManager atkM)
	{
		if (!IsBlocking) return false;
		object directionObj = atkM.GetData<DirectionComponent>()
			?? atkM.GetData<VelocityComponent>();
		if (directionObj == null) return false;
		Vector3 direction = (Vector3)directionObj;
		direction.Normalize();
		direction = -direction;
		float angle = Vector2.Angle(BlockDirection, direction);
		return angle <= blockMaxAngle;
	}

	protected virtual void TakeKnockbackHit(object knockbackDataObj, bool blocked)
	{
		if (knockbackDataObj == null) return;
		Vector3 knockback = (Vector3)knockbackDataObj;
		if (blocked)
		{
			knockback *= 0.5f;
		}
		PhysicsController.SetVelocity(knockback);
	}

	protected virtual void TakeStunHit(object stunDataObj, bool blocked)
	{
		if (blocked) return;
		if (stunDataObj == null) return;
		float stunDuration = (float)stunDataObj;
		float currentStunTime = TimerTracker.GetTimer(stunTimerID);
		if (currentStunTime >= stunDuration) return;
		TimerTracker.SetTimer(stunTimerID, stunDuration);
		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(stunDuration);
	}

	protected virtual void TakeDamage(object damageObj, bool blocked)
	{
		if (damageObj == null) return;
		float damage = (float)damageObj;
		if (blocked)
		{
			damage *= 1f - blockDamageReduction;
		}
		AddHealth(-damage);
	}

	protected virtual void HealthChanged(float previousValue, float currentValue)
	{
		if (currentValue <= 0f)
		{
			cac?.Die();
			DestroySelf();
		}
	}

	public virtual bool CanPerformAction
		=> !RecoveringFromAction
		&& !IsStunned
		&& !IsRolling
		&& !IsBlocking;

	protected virtual void Roll(Vector3 direction)
	{
		if (!CanRoll) return;

		TimerTracker.SetTimer(rollTimerID, rollDuration);
		if (IsRolling)
		{
			RollDirection = direction;
			DeactivateHitboxForDuration(rollIFrameDuration);
			PhysicsController.PreventMovementInputForDuration(rollDuration);
			cac?.SetRolling();
		}
	}

	private float RollTimer => TimerTracker.GetTimer(rollTimerID);

	protected bool IsRolling => RollTimer > 0f;

	protected bool CanRoll
		=> !IsStunned
		&& !RecoveringFromAction
		&& !IsRolling;

	protected virtual void Block(Vector3 direction)
	{
		if (!CanBlock) return;

		IsBlocking = true;
		cac?.SetBlocking(true);
		BlockDirection = direction;
	}

	protected virtual void StopBlocking()
	{
		if (!IsBlocking) return;

		IsBlocking = false;
		cac?.SetBlocking(false);
	}

	protected bool CanBlock
		=> !IsBlocking
		&& !IsStunned
		&& !RecoveringFromAction;

	protected Vector3 BlockDirection { get; set; }

	public void ReceiveAttack(AttackManager atkM) => TakeHit(atkM);

	public string LayerName => LayerMask.LayerToName(gameObject.layer);

	protected virtual void CheckItemUsageInput() { }

	protected void CheckItemUsage(int itemIndex)
	{
		if (!CanUseItem) return;

		List<ItemStack> stacks = storage.stacks;
		if (stacks[itemIndex].GetAmount() > 0)
		{
			Item.Type type = stacks[itemIndex].GetItemType();
			if (UseItem(type))
			{
				stacks[itemIndex].RemoveAmount(1);
				GameEvents.ItemUsed(type);
			}
		}
	}

	protected bool UseItem(Item.Type type)
	{
		bool used = false;
		switch (type)
		{
			default: break;
			case Item.Type.RepairKit:
				used = true;
				break;
		}
		if (used)
		{
			TimerTracker.SetTimer(itemUseCooldownTimerID, itemUseCooldownDuration);
			TimerTracker.SetTimer(actionRecoveryTimerID, itemUseRecoveryDuration);
			OnItemUsed?.Invoke(type);
		}
		return used;
	}

	public virtual int CollectItem(ItemStack stack)
	{
		Item.Type itemType = stack.GetItemType();
		int amount = stack.GetAmount();
		Inventory inv = GetAppropriateInventory(itemType);
		return amount - inv.AddItem(itemType, amount);
	}

	protected virtual Inventory GetAppropriateInventory(Item.Type itemType) => storage;

	private bool CanUseItem => !ItemUsageOnCooldown && !RecoveringFromAction;

	private bool ItemUsageOnCooldown => TimerTracker.GetTimer(itemUseCooldownTimerID) > 0f;
}
