using UnityEngine;

[RequireComponent(typeof(PlanetPlayerTriggerer))]
public class PlanetPlayer : PlanetRoomObject
{
	[SerializeField] private Inventory bag, keyItems;
	private PlanetPlayerTriggerer actor;
	private PlanetPlayerTriggerer Actor => actor ?? (actor = GetComponent<PlanetPlayerTriggerer>());
	private ItemPopupUI popupUI;
	private ItemPopupUI PopupUI => popupUI ?? (popupUI = FindObjectOfType<ItemPopupUI>());

	private void OnEnable() => PlanetGenerator.OnRoomChanged += ResetPosition;

	private void OnDisable() => PlanetGenerator.OnRoomChanged -= ResetPosition;

	private void ResetPosition(Room newRoom, Direction direction)
	{
		room = newRoom;
		Direction opposite = Room.Opposite(direction);
		Vector2 resetPos = room.GetExitPos(opposite);
		transform.position = resetPos;
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

	public bool RemoveKey(RoomKey.KeyColour colour)
		=> keyItems.RemoveItem(RoomKey.ConvertToItemType(colour), 1);
}
