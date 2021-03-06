﻿using UnityEngine;

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

		protected GatheringQReq() : base()
		{

		}

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
			if (Completed)
			{
				Debug.Log("Quest requirement already completed.");
				return;
			}

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
			string.Format(description, amountNeeded, typeNeeded.ItemName, currentAmount);

		private const string TYPE_NEEDED_VAR_NAME = "Type Needed",
			AMOUNT_NEEDED_VAR_NAME = "Amount Needed",
			CURRENT_PROGRESS_VAR_NAME = "Current Progress",
			INVENTORY_HOLDER_ID_VAR_NAME = "Inventory Holder ID";

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

		protected override bool ApplyData(DataModule module)
		{
			if (base.ApplyData(module)) return true;

			switch (module.parameterName)
			{
				default:
					return false;
				case TYPE_NEEDED_VAR_NAME:
					typeNeeded = Item.GetItemByName(module.data);
					if (typeNeeded == ItemObject.Blank)
					{
						Debug.Log("Type Needed data could not be parsed.");
					}
					break;
				case AMOUNT_NEEDED_VAR_NAME:
				{
					bool foundVal = int.TryParse(module.data, out int val);
					if (foundVal)
					{
						amountNeeded = val;
					}
					else
					{
						Debug.Log("Amount needed data could not be parsed.");
					}
				}
					break;
				case CURRENT_PROGRESS_VAR_NAME:
				{
					bool foundVal = int.TryParse(module.data, out int val);
					if (foundVal)
					{
						currentAmount = val;
					}
					else
					{
							Debug.Log("Current Progress data could not be parsed.");
					}
				}
					break;
				case INVENTORY_HOLDER_ID_VAR_NAME:
					InventoryHolderID = module.data;
					if (InventoryHolderID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(InventoryHolderID);
						if (obj is IInventoryHolder ih)
						{
							inventoryHolder = ih;
						}
					}
					break;
			}

			return true;
		}
	}
}