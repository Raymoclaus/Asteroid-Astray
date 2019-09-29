using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomLandingPad : PlanetNonSolid
{
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
