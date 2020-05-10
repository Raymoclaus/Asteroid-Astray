using InventorySystem;
using SaveSystem;

namespace QuestSystem
{
	public class ItemQReward : QuestReward
	{
		public ItemStack stack;

		public ItemQReward(string description, ItemStack stack) : base(description)
		{
			this.stack = stack;
		}

		public ItemQReward(ItemStack stack) : this("{0} (x{1})", stack)
		{

		}

		public override string GetRewardDescription()
			=> string.Format(Description, stack.ItemType.ItemName, stack.Amount);

		public override void GiveReward(Quester quester) => quester.ReceiveReward(stack);

		protected override string GetRewardType() => REWARD_TYPE;

		private const string REWARD_TYPE = "Item Reward",
			ITEM_STACK_VAR_NAME = "Item Stack";
		
		public override void Save(string filename, SaveTag parentTag)
		{
			base.Save(filename, parentTag);
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save item stack
			DataModule module = new DataModule(ITEM_STACK_VAR_NAME, stack);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}

}