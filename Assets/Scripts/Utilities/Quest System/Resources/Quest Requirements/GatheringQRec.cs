using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class GatheringQRec : QuestRequirement
	{
		public Item.Type typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private string formattedDescription = "{0}: {1} / {2}";
		private bool formatDescription;
		private Action<Item.Type, int> action;

		public GatheringQRec(Item.Type typeNeeded, int amountNeeded,
			string description, bool formatDescription, Action<Item.Type, int> action)
			: base(description.Replace("?", Item.TypeName(typeNeeded)), amountNeeded)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.formatDescription = formatDescription;
			this.action = action;
		}

		public override void Activate()
		{
			base.Activate();
			action += EvaluateEvent;
		}

		private void EvaluateEvent(Item.Type type, int amount)
		{
			if (Completed || !active) return;

			if (type == typeNeeded && amount != 0)
			{
				currentAmount += amount;
				QuestRequirementUpdated();
				if (currentAmount >= amountNeeded)
				{
					QuestRequirementCompleted();
					action -= EvaluateEvent;
				}
			}
		}

		public override string GetDescription() =>
			formatDescription ?
			string.Format(formattedDescription, description, currentAmount, amountNeeded)
			: description;

		public override Vector3? TargetLocation() => null;
	}

}