using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using TriggerSystem;

	public class TriggerQReq : QuestRequirement
	{
		private Action<IActor> action;
		private IActor actor;

		public TriggerQReq(Action<IActor> action, IActor actor, string description)
			: base(description)
		{
			this.action = action;
			this.actor = actor;
		}

		public override void Activate()
		{
			base.Activate();
			action += EvaluateEvent;
		}

		private void EvaluateEvent(IActor actor)
		{
			if (Completed || !active) return;

			if (this.actor == actor)
			{
				QuestRequirementCompleted();
				action -= EvaluateEvent;
			}
		}

		public override string GetDescription() => description;

		public override Vector3? TargetLocation() => trigger.GetPosition();
	}

}