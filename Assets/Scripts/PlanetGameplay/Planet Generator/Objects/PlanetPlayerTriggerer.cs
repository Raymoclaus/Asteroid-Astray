using UnityEngine;
using InputHandler;

[RequireComponent(typeof(PlanetPlayer))]
public class PlanetPlayerTriggerer : PlanetTriggerer
{
	private PlanetPlayer Player => (PlanetPlayer)RoomObj;

	public override bool IsInteracting(InteractablePromptTrigger trigger)
		=> InputManager.GetInput(trigger.Action);

	public override void Interacted(PlanetInteractable trigger)
	{
		if (trigger is PlanetRoomKey)
		{
			PlanetRoomKey key = (PlanetRoomKey)trigger;
			Player.CollectItem(key.GetItem());
			return;
		}

		if (trigger is PlanetRoomLock)
		{
			PlanetRoomLock planetLock = (PlanetRoomLock)trigger;
			if (Player.RemoveKeyFromInventory(planetLock.colour))
			{
				planetLock.Unlock();
			}
			return;
		}

		if (trigger is PlanetRoomPushableBlock)
		{
			PlanetRoomPushableBlock pushableBlock = (PlanetRoomPushableBlock)trigger;
			pushableBlock.Push(MovementBehaviour.DirectionValue);
			return;
		}
	}
}
