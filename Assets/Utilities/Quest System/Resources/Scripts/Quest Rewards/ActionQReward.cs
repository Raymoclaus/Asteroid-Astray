namespace QuestSystem
{
	public class ActionQReward : QuestReward
	{
		public ActionQReward(string description) : base(description)
		{

		}

		public override void GiveReward(Quester c)
		{

		}

		protected override string GetRewardType() => REWARD_TYPE;

		private const string REWARD_TYPE = "Action Reward";
	}
}