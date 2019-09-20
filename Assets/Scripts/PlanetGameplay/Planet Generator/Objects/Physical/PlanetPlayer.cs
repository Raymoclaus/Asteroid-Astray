using UnityEngine;
using InputHandler;

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

	protected override void Awake()
	{
		base.Awake();
		roomObject = new RoomPlayer();
		if (planetGenerator != null)
		{
			planetGenerator.OnRoomChanged += ResetPosition;
		}
	}

	private void OnDisable()
	{
		if (planetGenerator != null)
		{
			planetGenerator.OnRoomChanged -= ResetPosition;
		}
	}

	protected override bool ShouldAttack()
		=> base.ShouldAttack() && InputManager.GetInput("Attack");

	protected override void Attack()
	{
		GameObject attack = Instantiate(attackPrefab);

		AttackData.AttackManager atkM = attack.GetComponent<AttackData.AttackManager>();
		float damage = 15f;
		atkM.AddAttackComponent<AttackData.AttackDamageData>(damage);

		Vector3 direction = PhysicsController.GetDirection();
		Vector3 facingDirection = PhysicsController.GetFacingDirection();
		attack.transform.position =
			pivot.position + facingDirection;
		atkM.AddAttackComponent<AttackData.AttackDirectionData>(direction);
		atkM.AddAttackComponent<AttackData.AttackOwnerData>(this);
		atkM.AddAttackComponent<AttackData.AttackKnockbackData>(direction * damage);
		atkM.AddAttackComponent<AttackData.AttackStunData>(1f);

		attack.transform.parent = transform;
		attack.transform.eulerAngles =
			Vector3.forward * Vector2.SignedAngle(Vector2.up, facingDirection);

		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(new WaitForSeconds(0.5f));

		StartCoroutine(SetAttackCooldown(new WaitForSeconds(1f)));
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
