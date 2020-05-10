using System;
using System.Collections.Generic;
using SaveSystem;

namespace QuestSystem
{
	public class Quest
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public Quester QuestTaker { get; private set; }
		public List<QuestReward> Rewards { get; private set; }
		public List<QuestRequirement> Requirements { get; private set; }
		
		public event Action<Quest> OnQuestComplete;
		public void QuestComplete(Quest quest) => OnQuestComplete?.Invoke(quest);

		public Quest(string name, string description, Quester quester,
			List<QuestReward> rewards, List<QuestRequirement> requirements)
		{
			Name = name;
			Description = description;
			QuestTaker = quester;
			Rewards = rewards;
			Requirements = requirements;

			for (int i = 0; i < Requirements.Count; i++)
			{
				QuestRequirement requirement = Requirements[i];
				requirement.id = i;
				requirement.OnQuestRequirementCompleted += EvaluateRequirements;
			}
		}

		private void EvaluateRequirements()
		{
			if (!IsComplete) return;

			SteamPunkConsole.WriteLine($"Quest Complete: {Name}");
			QuestComplete(this);
			Rewards.ForEach(t => t.GiveReward(QuestTaker));
		}

		public bool IsComplete => !Requirements.Exists(t => !t.Completed);

		public void Activate()
		{
			SteamPunkConsole.WriteLine($"Quest Started: {Name}");
			Requirements.ForEach(t => t.Activate());
		}

		public void ForceComplete()
			=> Requirements.ForEach(t => t.QuestRequirementCompleted());

		public bool CompareName(Quest other) => Name == other.Name;

		protected string SaveTagName => $"Quest:{Name}";

		private const string QUEST_NAME_VAR_NAME = "Quest Name",
			DESCRIPTION_VAR_NAME = "Description";

		public void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save name
			DataModule module = new DataModule(QUEST_NAME_VAR_NAME, Name);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save description
			module = new DataModule(DESCRIPTION_VAR_NAME, Description);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save rewards
			foreach (QuestReward reward in Rewards)
			{
				reward.Save(filename, mainTag);
			}
			//save requirements
			foreach (QuestRequirement requirement in Requirements)
			{
				requirement.Save(filename, mainTag);
			}
		}
	}
}