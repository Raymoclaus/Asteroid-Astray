public abstract class QuestReward
{
	private const string NO_REWARD = "Reward not specified";

	public virtual string GetRewardName()
	{
		return NO_REWARD;
	}

	public virtual void GiveReward(IQuester e)
	{

	}
}
