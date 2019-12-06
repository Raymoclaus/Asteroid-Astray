using System;
using CustomDataTypes;
using InventorySystem;
using TriggerSystem.Actors.Interactors;
using UnityEngine;

[RequireComponent(typeof(DungeonRoomObjectComponent))]
public class PlanetPlayerInteractor : InputBasedInteractor
{
	private DungeonRoomObjectComponent droc;
	private DungeonRoomObjectComponent Droc => droc != null ? droc
		: (droc = GetComponent<DungeonRoomObjectComponent>());
	[SerializeField] private IInventoryHolder inventoryHolder;

	private void Start()
	{
		Droc.Viewer.OnRoomChanged += GoToEndOfExit;
	}

	private void GoToEndOfExit(DungeonRoom newRoom, Direction direction)
	{
		IntPair roomPos = newRoom.GetExitPos(direction.Opposite());
		Vector3 worldPos = newRoom.WorldSpacePosition + roomPos;
		transform.position = worldPos;
	}

	public override void Interact(object interactableObject)
	{
		base.Interact(interactableObject);

		if (interactableObject is PlanetExitTrigger exitTrigger)
		{
			DungeonRoom startRoom = exitTrigger.Droc.RoomObject.CurrentRoom;
			Direction direction = (Direction)exitTrigger.Droc.Data;
			Droc.Viewer.Go(startRoom, direction);
		}

		if (interactableObject is ItemObject itemType)
		{
			inventoryHolder.GiveItem(itemType);
		}
	}
}
