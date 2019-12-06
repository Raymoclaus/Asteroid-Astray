using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class ItemUseQReq : QuestRequirement
	{
		public ItemObject typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private IInventoryHolder inventoryHolder;

		public ItemUseQReq(ItemObject typeNeeded, int amountNeeded,
			IInventoryHolder inventoryHolder, string description = null)
			: base(description != null ? description : "Use {0} {1}: {2} / {0}", null)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.inventoryHolder = inventoryHolder;
		}

		public ItemUseQReq(ItemObject typeNeeded,
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

		private void EvaluateEvent(ItemObject type, int amount)
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
			=> string.Format(description, amountNeeded, typeNeeded, currentAmount);
	}
}