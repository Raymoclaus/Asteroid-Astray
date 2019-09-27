using UnityEngine;
using InputHandler;
using AttackData;

[RequireComponent(typeof(PlanetPlayerTriggerer))]
public class PlanetPlayer : PlanetRoomEntity
{
	[SerializeField] private Inventory bag, keyItems;
	private PlanetPlayerTriggerer actor;
	private PlanetPlayerTriggerer Actor
		=> actor ?? (actor = GetComponent<PlanetPlayerTriggerer>());
	private ItemPopupUI popupUI;
	private ItemPopupUI PopupUI
		=> popupUI ?? (popupUI = FindObjectOfType<ItemPopupUI>());
	[SerializeField] private GameObject attackPrefab;
	[SerializeField] private float rangedAttackCooldownDuration = 1f;
	private string rangedAttackCooldownTimerID;
	[SerializeField] private float rangedAttackRecoveryDuration = 0.5f;
	private string attackRecoveryTimerID;

	protected override void Awake()
	{
		base.Awake();

		roomObject = new RoomPlayer();
		if (planetGenerator != null)
		{
			planetGenerator.OnRoomChanged += ResetPosition;
		}
		rangedAttackCooldownTimerID = gameObject.GetInstanceID() + "Ranged Attack Cooldown Timer";
		TimerTracker.AddTimer(rangedAttackCooldownTimerID, 0f, null, null);
		attackRecoveryTimerID = gameObject.GetInstanceID() + "Attack Recovery Timer";
		TimerTracker.AddTimer(attackRecoveryTimerID, 0f, null, null);
	}

	protected override void Update()
	{
		base.Update();

		if (ShouldRangedAttack)
		{
			RangedAttack();
		}
	}

	private void OnDisable()
	{
		if (planetGenerator != null)
		{
			planetGenerator.OnRoomChanged -= ResetPosition;
		}
	}

	private bool ShouldAttack
		=> !RecoveringFromAttack
		&& !IsStunned;	   		

	private bool ShouldRangedAttack
		=> ShouldAttack
		&& InputManager.GetInput("RangedAttack") > 0f
		&& !RangedAttackOnCooldown;

	private bool RangedAttackOnCooldown
		=> TimerTracker.GetTimer(rangedAttackCooldownTimerID) > 0f;

	private bool RecoveringFromAttack => TimerTracker.GetTimer(attackRecoveryTimerID) > 0f;

	private void RangedAttack()
	{
		TimerTracker.SetTimer(rangedAttackCooldownTimerID, rangedAttackCooldownDuration);

		GameObject attack = Instantiate(attackPrefab);

		AttackManager atkM = attack.GetComponent<AttackManager>();
		float damage = 15f;
		atkM.AddAttackComponent<DamageComponent>(damage);

		Vector3 direction = PhysicsController.GetMovementDirection;
		Vector3 facingDirection = PhysicsController.GetFacingDirection;
		attack.transform.position =
			pivot.position + facingDirection;
		atkM.AddAttackComponent<DirectionComponent>(direction);
		atkM.AddAttackComponent<OwnerComponent>(this);
		atkM.AddAttackComponent<KnockbackComponent>(direction * damage);
		atkM.AddAttackComponent<StunComponent>(1f);
		atkM.AddAttackComponent<IsProjectileComponent>(true);
		float projectileSpeed = 20f;
		atkM.AddAttackComponent<VelocityComponent>(direction.normalized * projectileSpeed);
		LayerComponent.ComponentData layerMask = new LayerComponent.ComponentData("Wall");
		atkM.AddAttackComponent<DestroyOnContactWithLayersComponent>(layerMask);

		attack.transform.parent = transform;
		attack.transform.eulerAngles =
			Vector3.forward * Vector2.SignedAngle(Vector2.up, facingDirection);

		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(rangedAttackRecoveryDuration);
	}

	private void ResetPosition(Room newRoom, Direction direction)
	{
		room = newRoom;
		Vector2 offset = room.position * room.GetDimensions();
		Direction opposite = Room.Opposite(direction);
		Vector2 resetPos = room.GetExitPos(opposite);
		transform.position = resetPos + offset;
	}

	public void CollectItem(ItemStack stack)
	{
		Item.Type itemType = stack.GetItemType();
		int amount = stack.GetAmount();
		Inventory inv = Item.IsKeyItem(itemType) ? keyItems : bag;
		int collectedAmount = amount - inv.AddItem(itemType, amount);
		if (collectedAmount > 0)
		{
			GameEvents.ItemCollected(itemType, collectedAmount);
		}

		TriggerPopupUI(itemType, amount);
	}

	private void TriggerPopupUI(Item.Type type, int amount)
	{
		PopupUI?.GeneratePopup(type, amount);
	}

	public bool RemoveKeyFromInventory(RoomKey.KeyColour colour)
		=> keyItems.RemoveItem(RoomKey.ConvertToItemType(colour), 1);
}
