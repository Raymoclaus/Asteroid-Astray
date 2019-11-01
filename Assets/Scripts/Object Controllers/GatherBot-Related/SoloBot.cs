using UnityEngine;

public class SoloBot : GatherBot
{
	protected override void OnSpawn()
	{
		SetState(AIState.Scanning);
		Activate(true);
		hive = null;
	}

	protected override void SetState(AIState newState)
	{
		switch (newState)
		{
			default:
				base.SetState(newState);
				break;
			case AIState.Spawning:
			case AIState.Exploring:
			case AIState.Storing:
			case AIState.Signalling:
				SetState(AIState.Scanning);
				break;
		}
	}

	protected override void Wandering() => SetState(AIState.Scanning);
	
	protected override AttackViability EvaluateScan(Scan sc)
	{
		AttackViability scanResult = base.EvaluateScan(sc);
		switch (scanResult)
		{
			case AttackViability.SignalForHelp:
				scanResult = AttackViability.Escape;
				break;
		}
		return scanResult;
	}

	protected override void AlertAll(ICombat threat) => Alert(threat);
}
