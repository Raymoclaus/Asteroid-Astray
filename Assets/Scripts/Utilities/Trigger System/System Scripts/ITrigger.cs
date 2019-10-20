using System;
using UnityEngine;

namespace TriggerSystem
{
	public interface ITrigger
	{
		event Action<IActor> OnEnteredTrigger;
		event Action<IActor> OnExitedTrigger;
		event Action OnAllExitedTrigger;
		int TriggerLayer { get; }
		bool TriggerEnabled { get; set; }
		Vector3 PivotPosition { get; }
		void SetCollider(Collider2D col);
		void SetPivot(Transform t);
	}
}