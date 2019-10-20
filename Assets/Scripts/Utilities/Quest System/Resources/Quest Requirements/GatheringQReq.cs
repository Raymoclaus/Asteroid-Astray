using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class GatheringQReq : QuestRequirement
	{
		public Item.Type typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private const string formattedDescription = "{0}: {1} / {2}";
		private IInventoryHolder inventoryHolder;

		public GatheringQReq(Item.Type typeNeeded, int amountNeeded,
			IInventoryHolder inventoryHolder, string description = null)
			: base(string.IsNullOrWhiteSpace(description)
				  ? $"Gather # {typeNeeded}" : description, amountNeeded)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.inventoryHolder = inventoryHolder;
		}

		public GatheringQReq(Item.Type typeNeeded, IInventoryHolder inventoryHolder,
			string description = null)
			: this(typeNeeded, 1, inventoryHolder, description)
		{

		}

		public override void Activate()
		{
			base.Activate();
			inventoryHolder.OnItemCollected += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			inventoryHolder.OnItemCollected -= EvaluateEvent;
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
				}
			}
		}

		public override string GetDescription =>
			string.Format(formattedDescription, description, currentAmount, amountNeeded);

		public override IWaypoint GetWaypoint => null;
	}

}