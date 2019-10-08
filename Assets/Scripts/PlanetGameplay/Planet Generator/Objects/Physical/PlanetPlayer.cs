using UnityEngine;
using InputHandler;
using AttackData;

[RequireComponent(typeof(PlanetPlayerTriggerer))]
public class PlanetPlayer : PlanetRoomEntity
{
	[Header("Player Entity Fields")]
	[SerializeField] private Inventory keyItems;
	private PlanetPlayerTriggerer actor;
	private PlanetPlayerTriggerer Actor
		=> actor ?? (actor = GetComponent<PlanetPlayerTriggerer>());
	private ItemPopupUI popupUI;
	private ItemPopupUI PopupUI
		=> popupUI ?? (popupUI = FindObjectOfType<ItemPopupUI>());
	[Header("Behaviour Fields")]
	[SerializeField] private InputRollingBehaviour rollingBehaviour;
	[SerializeField] private InputBlockBehaviour blockingBehaviour;

	[Header("Ranged Attack Fields")]
	[SerializeField] private AttackManager rangedAttackPrefab;
	[SerializeField] private float rangedAttackCooldownDuration = 1f;
	private string rangedAttackCooldownTimerID;
	[SerializeField] private float rangedAttackRecoveryDuration = 0.5f;
	[SerializeField] private float rangedAttackDamage = 15f;
	[SerializeField] private float rangedAttackStunDuration = 0.3f;
	[SerializeField] private float rangedAttackProjectileSpeed = 20f;

	[Header("Melee Attack Fields")]
	[SerializeField] private AttackManager meleeAttackPrefab;
	[SerializeField] private float meleeAttackCooldownDuration = 0.3f;
	private string meleeAttackCooldownTimerID;
	[SerializeField] private float meleeAttackRecoveryDuration = 0.2f;
	[SerializeField] private float meleeAttackDamage = 40f;
	[SerializeField] private float meleeAttackStunDuration = 0.7f;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		roomViewer.OnRoomChanged += ResetPosition;

		rollingBehaviour.OnRoll += Roll;
		blockingBehaviour.OnBlock += Block;
		blockingBehaviour.OnStopBlocking += StopBlocking;

		rangedAttackCooldownTimerID = gameObject.GetInstanceID() + "Ranged Attack Cooldown Timer";
		TimerTracker.AddTimer(rangedAttackCooldownTimerID, 0f, null, null);
		meleeAttackCooldownTimerID = gameObject.GetInstanceID() + "Melee Attack Cooldown Timer";
		TimerTracker.AddTimer(meleeAttackCooldownTimerID, 0f, null, null);
	}

	protected override void Update()
	{
		base.Update();

		if (ShouldRangedAttack)
		{
			RangedAttack();
		}

		if (ShouldMeleeAttack)
		{
			MeleeAttack();
		}
	}

	private void OnDisable()
	{
		if (roomViewer != null)
		{
			roomViewer.OnRoomChanged -= ResetPosition;
		}
	}

	public bool CanInteract
		=> CanPerformAction;

	private bool ShouldAttack
		=> !RecoveringFromAction
		&& !IsStunned
		&& !IsRolling;

	private bool ShouldMeleeAttack
		=> ShouldAttack
		&& InputManager.GetInputDown("Melee Attack")
		&& !MeleeAttackOnCooldown;

	private bool ShouldRangedAttack
		=> ShouldAttack
		&& InputManager.GetInputDown("Ranged Attack")
		&& !RangedAttackOnCooldown;

	private bool RangedAttackOnCooldown
		=> TimerTracker.GetTimer(rangedAttackCooldownTimerID) > 0f;

	private bool MeleeAttackOnCooldown
		=> TimerTracker.GetTimer(meleeAttackCooldownTimerID) > 0f;

	private void MeleeAttack()
	{
		StopBlocking();
		TimerTracker.SetTimer(meleeAttackCooldownTimerID, meleeAttackCooldownDuration);
		TimerTracker.SetTimer(actionRecoveryTimerID, meleeAttackRecoveryDuration);

		AttackManager atkM = Instantiate(meleeAttackPrefab);

		atkM.AddAttackComponent<DamageComponent>(meleeAttackDamage);

		Vector3 direction = PhysicsController.MovementDirection;
		atkM.transform.position =
			pivot.position + direction;
		atkM.AddAttackComponent<DirectionComponent>(direction);
		atkM.AddAttackComponent<OwnerComponent>(this);
		atkM.AddAttackComponent<KnockbackComponent>(direction * meleeAttackDamage);
		atkM.AddAttackComponent<StunComponent>(meleeAttackStunDuration);
		atkM.AddAttackComponent<DestroyAfterTimeComponent>(0.2f);
		atkM.AddAttackComponent<KnockbackComponent>(direction * meleeAttackDamage);

		atkM.transform.parent = transform;
		atkM.transform.eulerAngles =
			Vector3.forward * Vector2.SignedAngle(Vector2.up, direction);

		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(rangedAttackRecoveryDuration);
	}

	private void RangedAttack()
	{
		StopBlocking();
		TimerTracker.SetTimer(rangedAttackCooldownTimerID, rangedAttackCooldownDuration);

		AttackManager atkM = Instantiate(rangedAttackPrefab);

		atkM.AddAttackComponent<DamageComponent>(rangedAttackDamage);

		Vector3 direction = PhysicsController.MovementDirection;
		Vector3 facingDirection = PhysicsController.FacingDirection;
		atkM.transform.position =
			pivot.position + facingDirection;
		atkM.AddAttackComponent<DirectionComponent>(direction);
		atkM.AddAttackComponent<OwnerComponent>(this);
		atkM.AddAttackComponent<KnockbackComponent>(direction * rangedAttackDamage);
		atkM.AddAttackComponent<StunComponent>(rangedAttackStunDuration);
		atkM.AddAttackComponent<IsProjectileComponent>(true);
		atkM.AddAttackComponent<VelocityComponent>(
			direction.normalized * rangedAttackProjectileSpeed);
		LayerComponent.ComponentData layerMask = new LayerComponent.ComponentData("Wall");
		atkM.AddAttackComponent<DestroyOnContactWithLayersComponent>(layerMask);

		atkM.transform.parent = transform;
		atkM.transform.eulerAngles =
			Vector3.forward * Vector2.SignedAngle(Vector2.up, facingDirection);

		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(rangedAttackRecoveryDuration);
	}

	private void ResetPosition(Room newRoom, Direction direction)
	{
		room = newRoom;
		IntPair intOffset = room.position * room.Dimensions;
		Vector2 offset = new Vector2(intOffset.x, intOffset.y);
		Vector2 resetPos = room.GetExitPos(direction.Opposite());
		transform.position = resetPos + offset;
	}

	private void TriggerPopupUI(Item.Type type, int amount)
	{
		PopupUI?.GeneratePopup(type, amount);
	}

	protected override void CheckItemUsageInput()
	{
		base.CheckItemUsageInput();

		for (int i = 0; i < 8; i++)
		{
			if (InputManager.GetInput($"Slot {i + 1}") > 0f)
			{
				CheckItemUsage(i);
			}
		}
	}

	public override int CollectItem(ItemStack stack)
	{
		Item.Type itemType = stack.GetItemType();
		int collectedAmount = base.CollectItem(stack);

		if (collectedAmount > 0)
		{
			GameEvents.ItemCollected(itemType, collectedAmount);
		}
		TriggerPopupUI(itemType, collectedAmount);
		return collectedAmount;
	}

	protected override Inventory GetAppropriateInventory(Item.Type itemType)
		=> Item.IsKeyItem(itemType) ? keyItems : base.GetAppropriateInventory(itemType);

	public override bool RemoveFromInventory(ItemStack stack)
		=> base.RemoveFromInventory(stack)
		|| keyItems.RemoveItem(stack.GetItemType(), stack.GetAmount());
}
