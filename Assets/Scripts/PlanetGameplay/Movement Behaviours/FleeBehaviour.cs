using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeBehaviour : TargetBasedBehaviour
{
	public override void TriggerUpdate()
	{
		base.TriggerUpdate();

		if (TargetIsNearby && ShouldRunAway)
		{
			MoveAwayFromPosition(TargetPosition);
		}
		else
		{
			SlowDown();
		}
	}

	protected virtual bool ShouldRunAway => true;
}
