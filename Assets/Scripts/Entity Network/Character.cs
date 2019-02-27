public class Character : Entity
{
	public virtual void ReceiveItemReward(Item.Type type, int amount)
	{
		CollectResources(type, amount);
	}

	public virtual void AcceptQuest(Quest quest)
	{
		quest.Activate();
	}
}
