using UnityEngine;
using InputHandler;

[RequireComponent(typeof(PlanetPlayer))]
public class PlanetPlayerTriggerer : PlanetTriggerer
{
	private PlanetPlayer Player => (PlanetPlayer)RoomObj;

	protected override bool IsPerformingAction(string action)
		=> InputManager.GetInputDown(action);

	public override void Interacted(PlanetInteractable trigger)
	{
		if (trigger is PlanetRoomKey)
		{
			PlanetRoomKey key = (PlanetRoomKey)trigger;
			Player.CollectItem(key.GetItem());
			return;
		}

		if (trigger is PlanetRoomPushableBlock)
		{
			PlanetRoomPushableBlock pushableBlock = (PlanetRoomPushableBlock)trigger;
			pushableBlock.Push(MovementBehaviour.DirectionValue);
			return;
		}
	}

	public override bool RequestObject(object objectToGive)
	{
		if (base.RequestObject(objectToGive)) return true;

		if (objectToGive is ItemStack)
		{
			ItemStack stack = (ItemStack)objectToGive;
			return Player.RemoveFromInventory(stack);
		}

		return false;
	}
}
