using UnityEngine;
using InventorySystem;
using MovementBehaviours;

[RequireComponent(typeof(ItemPickup))]
[RequireComponent(typeof(FollowBehaviour))]
public class ChasingItemPickup : MonoBehaviour
{
	private FollowBehaviour follow;
	private FollowBehaviour Follow => follow != null ? follow
		: (follow = GetComponent<FollowBehaviour>());
	private ItemPickup pickup;
	public ItemPickup Pickup => pickup != null ? pickup
		: (pickup = GetComponent<ItemPickup>());

	private IInventoryHolder targetInventoryHolder;

	private void Awake()
	{
		Follow.OnReachedTarget += GiveItem;
	}

	private void Update() => Follow.TriggerUpdate();

	public void SetTarget(IInventoryHolder inventoryHolder)
	{
		targetInventoryHolder = inventoryHolder;
		Follow.SetTarget(inventoryHolder.GetTransform);
	}

	private void GiveItem() => Pickup.SendItem(targetInventoryHolder);
}
