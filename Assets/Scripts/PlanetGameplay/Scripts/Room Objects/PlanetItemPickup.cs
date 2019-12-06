using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TriggerSystem;
using TriggerSystem.MessageReceivers;
using UnityEngine;

[RequireComponent(typeof(DungeonRoomObjectComponent))]
public class PlanetItemPickup : DestructibleInteractableObject
{
	private DungeonRoomObjectComponent droc;
	public DungeonRoomObjectComponent Droc => droc != null ? droc
		: (droc = GetComponent<DungeonRoomObjectComponent>());

	public ItemObject itemType;

	protected override void PerformAction(IInteractor interactor)
	{
		interactor.Interact(itemType);
		base.PerformAction(interactor);
	}
}
