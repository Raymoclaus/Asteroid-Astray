using UnityEngine;

[RequireComponent(typeof(PlanetPlayer))]
public class PlanetPlayerTriggerer : PlanetTriggerer
{
	private PlanetPlayer player;
	private PlanetPlayer Player => player ?? (player = GetComponent<PlanetPlayer>());

	public override bool IsInteracting(InteractablePromptTrigger trigger)
	{
		return InputHandler.GetInputDown(trigger.Action) != 0f;
	}

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
			if (Player.RemoveKey(planetLock.colour))
			{
				planetLock.Unlock();
			}
			return;
		}
	}
}
