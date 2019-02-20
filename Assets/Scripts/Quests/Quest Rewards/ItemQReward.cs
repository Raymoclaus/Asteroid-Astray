public class ItemQReward : QuestReward
{
	public Item.Type type;
	public int amount;
	private string formattedString = "{0} (x{1})";

	public ItemQReward(Item.Type type, int amount)
	{
		this.type = type;
		this.amount = amount;
	}

	public override string GetRewardName()
	{
		return string.Format(formattedString, Item.TypeName(type), amount);
	}

	public override void GiveReward(Character c)
	{
		c.ReceiveItemReward(type, amount);
	}
}
