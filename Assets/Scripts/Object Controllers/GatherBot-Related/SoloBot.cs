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
			case AIState.Scanning:
			case AIState.Gathering:
			case AIState.Suspicious:
			case AIState.Attacking:
			case AIState.Collecting:
			case AIState.Escaping:
			case AIState.Dying:
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


	//codes: 0 = attack alone, 1 = signal for help, 2 = escape, 3 = ignore
	protected override int EvaluateScan(Scan sc)
	{
		int scanResult = base.EvaluateScan(sc);
		switch (scanResult)
		{
			case 1:
				scanResult = 2;
				break;
		}
		return scanResult;
	}

	protected override void AlertAll(ICombat threat) => Alert(threat);
}
