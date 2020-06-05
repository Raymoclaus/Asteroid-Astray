using SaveSystem;
using System.Collections.Generic;

namespace QuestSystem.Requirements
{
	using InventorySystem;
	using UnityEngine;

	public class CraftingQReq : QuestRequirement
	{
		public ItemObject typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private ICrafter crafter;
		private string CrafterID { get; set; }

		protected CraftingQReq() : base()
		{

		}

		public CraftingQReq(ItemObject typeNeeded, int amountNeeded,
			ICrafter crafter, string description = null)
			: base(string.IsNullOrWhiteSpace(description)
				  ? "Craft {0} {1}: {2} / {0}" : description, null)
		{
			this.typeNeeded = typeNeeded;
			this.amountNeeded = amountNeeded;
			this.crafter = crafter;
			CrafterID = crafter.UniqueID;
		}

		public CraftingQReq(ItemObject typeNeeded, ICrafter crafter,
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
			if (Completed)
			{
				Debug.Log("Quest requirement already completed.");
				return;
			}

			if (CrafterID != crafter.UniqueID) return;

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
			=> string.Format(description, amountNeeded, typeNeeded.ItemName, currentAmount);

		private const string TYPE_NEEDED_VAR_NAME = "Type Needed",
			AMOUNT_NEEDED_VAR_NAME = "Amount Needed",
			CURRENT_PROGRESS_VAR_NAME = "Current Progress",
			CRAFTER_ID_VAR_NAME = "Crafter ID";

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
			//save crafter ID
			module = new DataModule(CRAFTER_ID_VAR_NAME, CrafterID);
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
				case CRAFTER_ID_VAR_NAME:
					CrafterID = module.data;
					if (CrafterID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(CrafterID);
						if (obj is ICrafter c)
						{
							crafter = c;
						}
					}
					break;
			}

			return true;
		}
	}
}