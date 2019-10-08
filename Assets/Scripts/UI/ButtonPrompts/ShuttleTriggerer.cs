using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputHandler;

[RequireComponent(typeof(Shuttle))]
public class ShuttleTriggerer : Triggerer
{
	private Shuttle player;
	private Shuttle Player => player ?? (player = GetComponent<Shuttle>());

	public override void EnteredTrigger(VicinityTrigger vTrigger) => base.EnteredTrigger(vTrigger);

	public override void ExitedTrigger(VicinityTrigger vTrigger) => base.ExitedTrigger(vTrigger);

	protected override bool IsPerformingAction(string action)
		=> InputManager.GetInputDown(action);
}
