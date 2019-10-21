using InventorySystem;

namespace QuestSystem
{
	public class ItemQReward : QuestReward
	{
		public ItemStack stack;
		private string formattedString = "{0} (x{1})";

		public ItemQReward(ItemStack stack) => this.stack = stack;

		public override string GetRewardName()
			=> string.Format(formattedString, Item.TypeName(stack.GetItemType()), stack.GetAmount());

		public override void GiveReward(Quester quester) => quester.ReceiveReward(stack);
	}

}