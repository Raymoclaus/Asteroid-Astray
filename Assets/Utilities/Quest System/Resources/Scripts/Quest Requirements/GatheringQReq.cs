using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;
	using SaveSystem;

	public class GatheringQReq : QuestRequirement
	{
		public ItemObject typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private IInventoryHolder inventoryHolder;
		private string InventoryHolderID { get; set; }

		public GatheringQReq(ItemObject typeNeeded, int amountNeeded,
			IInventoryHolder inventoryHolder, string description = null, IWaypoint waypoint = null)
			: base(string.IsNullOrWhiteSpace(description)
				  ? "Gather {0} {1}: {2} / {0}" : description, waypoint)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.inventoryHolder = inventoryHolder;
			InventoryHolderID = inventoryHolder.UniqueID;
		}

		public GatheringQReq(ItemObject typeNeeded, IInventoryHolder inventoryHolder,
			string description = null, IWaypoint waypoint = null)
			: this(typeNeeded, 1, inventoryHolder, description, waypoint)
		{

		}

		public override void Activate()
		{
			base.Activate();
			inventoryHolder.OnItemsCollected += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			inventoryHolder.OnItemsCollected -= EvaluateEvent;
		}

		private void EvaluateEvent(ItemObject type, int amount)
		{
			if (Completed || !active) return;

			if (inventoryHolder.UniqueID != InventoryHolderID) return;

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
			string.Format(description, amountNeeded, Item.TypeName(typeNeeded), currentAmount);

		private const string SAVE_TAG_NAME = "Gathering Requirement";
		public override void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save description
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, description);
			//save waypoint ID
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, WaypointID);
			//save item type
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, typeNeeded);
			//save amount needed
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, amountNeeded);
			//save current progress
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, currentAmount);
			//save crafter ID
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, InventoryHolderID);
		}
	}
}