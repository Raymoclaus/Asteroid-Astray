using SaveSystem;
using System;
using System.Collections.Generic;

namespace QuestSystem
{
	public class Quest
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public Quester QuestTaker { get; set; }
		public List<QuestReward> Rewards { get; private set; }
		public List<QuestRequirement> Requirements { get; private set; }
		
		public event Action<Quest> OnQuestComplete;
		public void QuestComplete(Quest quest) => OnQuestComplete?.Invoke(quest);

		private Quest()
		{
			Rewards = new List<QuestReward>();
			Requirements = new List<QuestRequirement>();
		}

		public Quest(string name, string description, List<QuestReward> rewards, List<QuestRequirement> requirements)
		{
			Name = name;
			Description = description;
			Rewards = rewards;
			Requirements = requirements;

			for (int i = 0; i < Requirements.Count; i++)
			{
				QuestRequirement requirement = Requirements[i];
				requirement.id = i;
				requirement.OnQuestRequirementCompleted += EvaluateRequirements;
			}
		}

		private void SubscribeToRequirements()
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				QuestRequirement requirement = Requirements[i];
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

		protected string SaveTagName => $"{SAVE_TAG_NAME}:{Name}";

		private const string SAVE_TAG_NAME = "Quest",
			QUEST_NAME_VAR_NAME = "Quest Name",
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

		public static bool RecogniseTag(SaveTag tag)
		{
			return tag.TagName.StartsWith(SAVE_TAG_NAME);
		}

		public static Quest LoadQuestFromFile(string filename, SaveTag tag)
		{
			Quest q = new Quest();

			UnifiedSaveLoad.IterateTagContents(
				filename,
				tag,
				parameterCallBack: module => q.ApplyData(module),
				subtagCallBack: subtag => q.CheckSubtag(filename, subtag));

			q.SubscribeToRequirements();

			return q;
		}

		public bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
				case QUEST_NAME_VAR_NAME:
					Name = module.data;
					break;
				case DESCRIPTION_VAR_NAME:
					Description = module.data;
					break;
			}

			return true;
		}

		public bool CheckSubtag(string filename, SaveTag subtag)
		{
			if (QuestRequirement.RecogniseTag(subtag))
			{
				QuestRequirement qr = QuestRequirement.LoadQuestRequirementFromFile(filename, subtag);
				if (qr != null)
				{
					Requirements.Add(qr);
				}
				return true;
			}
			//else if (QuestReward.RecogniseTag(subtag))
			//{
			//	QuestReward qr = QuestReward.LoadQuestRewardFromFile(filename, subtag);
			//	Rewards.Add(qr);
			//	return true;
			//}
			else
			{
				return false;
			}
		}
	}
}