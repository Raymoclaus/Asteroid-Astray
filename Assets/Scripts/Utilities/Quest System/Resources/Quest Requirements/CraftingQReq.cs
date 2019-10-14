using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class CraftingQReq : QuestRequirement
	{
		public Item.Type typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private const string formattedDescription = "{0}: {1} / {2}";
		private Action<Item.Type, int> action;

		public CraftingQReq(Item.Type typeNeeded, int amountNeeded,
			Action<Item.Type, int> action)
			: base($"Craft # {typeNeeded}", amountNeeded)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.action = action;
		}

		public CraftingQReq(Item.Type typeNeeded, Action<Item.Type, int> action)
			: this(typeNeeded, 1, action)
		{

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

		public override string GetDescription()
		{
			return string.Format(formattedDescription, description, currentAmount, amountNeeded);
		}

		public override Vector3? TargetLocation()
		{
			return null;
		}
	}

}