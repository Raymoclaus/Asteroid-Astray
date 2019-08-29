using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomLandingPad : PlanetNonSolid
{
	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		OpenPrompt();
	}

	private void OpenPrompt()
	{
		ExitPlanetPrompt.ActivatePrompt();
	}
}
