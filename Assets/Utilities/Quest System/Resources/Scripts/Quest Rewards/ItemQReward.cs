using InventorySystem;
using SaveSystem;

namespace QuestSystem
{
	public class ItemQReward : QuestReward
	{
		public ItemStack stack;
		private string formattedString = "{0} (x{1})";

		public ItemQReward(ItemStack stack) => this.stack = stack;

		public override string GetRewardName()
			=> string.Format(formattedString, Item.TypeName(stack.ItemType), stack.Amount);

		public override void GiveReward(Quester quester) => quester.ReceiveReward(stack);

		private const string SAVE_TAG_NAME = "Item Reward";
		public override void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save item
			stack.Save(mainTag);
		}
	}

}