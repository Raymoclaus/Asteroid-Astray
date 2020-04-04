using System.Collections.Generic;
using SaveSystem;

namespace QuestSystem.Requirements
{
	using InventorySystem;

	public class CraftingQReq : QuestRequirement
	{
		public ItemObject typeNeeded;
		public int amountNeeded = 1;
		private int currentAmount = 0;
		private ICrafter crafter;
		private string CrafterID { get; set; }

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
			if (Completed || !active) return;

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
			=> string.Format(description, amountNeeded, Item.TypeName(typeNeeded), currentAmount);

		private const string SAVE_TAG_NAME = "Crafting Requirement";
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
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, CrafterID);
		}
	}
}