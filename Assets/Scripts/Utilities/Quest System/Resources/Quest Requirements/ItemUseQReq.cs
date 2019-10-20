using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class ItemUseQReq : QuestRequirement
	{
		public Item.Type typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private IInventoryHolder inventoryHolder;
		private const string formattedDescription = "{0}: {1} / {2}";

		public ItemUseQReq(Item.Type typeNeeded, int amountNeeded,
			IInventoryHolder inventoryHolder, string description = null)
			: base(description != null ? description : $"Use # {typeNeeded}", amountNeeded)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.inventoryHolder = inventoryHolder;
		}

		public ItemUseQReq(Item.Type typeNeeded,
			IInventoryHolder inventoryHolder, string description = null)
			: this(typeNeeded, 1, inventoryHolder, description)
		{

		}

		public override void Activate()
		{
			base.Activate();
			inventoryHolder.OnItemUsed += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			inventoryHolder.OnItemUsed -= EvaluateEvent;
		}

		private void EvaluateEvent(Item.Type type, int amount)
		{
			if (Completed || !active) return;

			if (type == typeNeeded)
			{
				currentAmount++;
				QuestRequirementUpdated();
				if (currentAmount >= amountNeeded)
				{
					QuestRequirementCompleted();
				}
			}
		}

		public override string GetDescription
			=> string.Format(formattedDescription, description,
				currentAmount, amountNeeded);
	}

}