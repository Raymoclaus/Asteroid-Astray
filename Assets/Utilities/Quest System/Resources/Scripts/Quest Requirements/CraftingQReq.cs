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
		private const string formattedDescription = "{0}: {1} / {2}";
		private bool shouldFormatDescription;
		private ICrafter crafter;

		public CraftingQReq(Item.Type typeNeeded, int amountNeeded,
			ICrafter crafter, string description = null)
			: base(string.IsNullOrWhiteSpace(description)
				  ? $"Craft # {typeNeeded}" : description, amountNeeded)
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
				if (stacks[i].GetItemType() == typeNeeded)
				{
					currentAmount += stacks[i].GetAmount();
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
			=> string.Format(formattedDescription, description, currentAmount, amountNeeded);

		public override IWaypoint GetWaypoint => null;
	}

}