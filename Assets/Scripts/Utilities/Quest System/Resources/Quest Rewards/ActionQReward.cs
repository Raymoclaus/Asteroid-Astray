namespace QuestSystem
{
	public class ActionQReward : QuestReward
	{
		private string description;

		public delegate void ActionRewardEventHandler(Quester c);
		private event ActionRewardEventHandler OnActionReward;
		private void ActionReward(Quester c) => OnActionReward?.Invoke(c);

		public ActionQReward(ActionRewardEventHandler action, string description)
		{
			OnActionReward += action;
			this.description = description;
		}

		public override string GetRewardName()
		{
			return description;
		}

		public override void GiveReward(Quester c)
		{
			ActionReward(c);
		}
	}

}