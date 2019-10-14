using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using TriggerSystem;

	public class InteractionQReq : QuestRequirement
	{
		private IActionTrigger trigger;
		private Action<IInteractor> action;
		private IInteractor actor;
		private Vector3? location;

		public InteractionQReq(IActionTrigger trigger, IInteractor actor,
			Action<IInteractor> action, string description)
			: base(description)
		{
			this.trigger = trigger;
			this.actor = actor;
			this.action = action;
		}

		public InteractionQReq(IInteractor actor,
			Action<IInteractor> action, string description)
			: base(description)
		{
			this.actor = actor;
			this.action = action;
		}

		public override void Activate()
		{
			base.Activate();
			action += EvaluateEvent;
		}

		private void EvaluateEvent(IInteractor actor)
		{
			if (Completed || !active || this.actor != actor) return;

			QuestRequirementCompleted();
			action -= EvaluateEvent;
		}

		public override string GetDescription() => description;

		public override Vector3? TargetLocation()
			=> trigger?.PivotPosition;
	}

}