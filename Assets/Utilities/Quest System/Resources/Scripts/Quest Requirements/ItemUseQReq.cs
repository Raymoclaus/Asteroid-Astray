using UnityEngine;
using System;

namespace QuestSystem.Requirements
{
	using InventorySystem;
	using SaveSystem;

	public class ItemUseQReq : QuestRequirement
	{
		public ItemObject typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private IInventoryHolder inventoryHolder;
		private string InventoryHolderID { get; set; }

		public ItemUseQReq(ItemObject typeNeeded, int amountNeeded,
			IInventoryHolder inventoryHolder, string description = null)
			: base(description != null ? description : "Use {0} {1}: {2} / {0}", null)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.inventoryHolder = inventoryHolder;
			InventoryHolderID = inventoryHolder.UniqueID;
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

			if (inventoryHolder.UniqueID == InventoryHolderID)

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
			=> string.Format(description, amountNeeded, typeNeeded.ItemName, currentAmount);

		private const string REQUIREMENT_TYPE = "Item Use Requirement",
			TYPE_NEEDED_VAR_NAME = "Type Needed",
			AMOUNT_NEEDED_VAR_NAME = "Amount Needed",
			CURRENT_PROGRESS_VAR_NAME = "Current Progress",
			INVENTORY_HOLDER_ID_VAR_NAME = "Crafter ID";

		public override string GetRequirementType() => REQUIREMENT_TYPE;

		public override void Save(string filename, SaveTag parentTag)
		{
			base.Save(filename, parentTag);

			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save item type
			DataModule module = new DataModule(TYPE_NEEDED_VAR_NAME, typeNeeded.GetTypeName());
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save amount needed
			module = new DataModule(AMOUNT_NEEDED_VAR_NAME, amountNeeded);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save current progress
			module = new DataModule(CURRENT_PROGRESS_VAR_NAME, currentAmount);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save inventory holder ID
			module = new DataModule(INVENTORY_HOLDER_ID_VAR_NAME, InventoryHolderID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}
}