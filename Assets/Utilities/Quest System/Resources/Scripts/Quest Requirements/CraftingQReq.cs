using UnityEngine;
using System;
using System.Collections.Generic;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class CraftingQReq : QuestRequirement
	{
		public Item.Type typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private ICrafter crafter;

		public CraftingQReq(Item.Type typeNeeded, int amountNeeded,
			ICrafter crafter, string description = null)
			: base(string.IsNullOrWhiteSpace(description)
				  ? "Craft {0} {1}: {2} / {0}" : description, null)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.crafter = crafter;
		}

		public CraftingQReq(Item.Type typeNeeded, ICrafter crafter,
			string description = null)
			: this(typeNeeded, 1, crafter, description)
		{

		}

		public override void Activate()
		{
			base.Activate();
			crafter.OnItemsCrafted += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			crafter.OnItemsCrafted -= EvaluateEvent;
		}

		private void EvaluateEvent(List<ItemStack> stacks)
		{
			if (Completed || !active) return;

			bool updateRequirement = false;
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType== typeNeeded)
				{
					currentAmount += stacks[i].Amount;
					updateRequirement = true;
				}
			}

			if (updateRequirement)
			{
				QuestRequirementUpdated();
			}

			if (currentAmount >= amountNeeded)
			{
				QuestRequirementCompleted();
			}
		}

		public override string GetDescription
			=> string.Format(description, amountNeeded, typeNeeded, currentAmount);
	}
}