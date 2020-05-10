using SaveSystem;

namespace QuestSystem
{
	public abstract class QuestReward
	{
		protected string Description { get; private set; }

		public QuestReward(string description)
		{
			Description = description;
		}

		public virtual string GetRewardDescription() => Description;

		public abstract void GiveReward(Quester c);

		protected abstract string GetRewardType();

		public string SaveTagName => $"QuestReward:{Description}";

		private const string REWARD_TYPE_VAR_NAME = "Reward Type",
			DESCRIPTION_VAR_NAME = "Description";

		public virtual void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save reward type
			DataModule module = new DataModule(REWARD_TYPE_VAR_NAME, GetRewardType());
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save description
			module = new DataModule(DESCRIPTION_VAR_NAME, Description);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}
}