using SaveSystem;
using System;
using UnityEngine;

namespace QuestSystem
{
	public class Quester : MonoBehaviour
	{
		private QuestLog questLog = new QuestLog();
		public event Action<Quest> OnQuestAccepted, OnQuestCompleted, OnTopPriorityQuestSet;
		public event Action<object> OnRewardReceived;
		public Quest TopPriorityQuest { get; set; }
		private string TopPriorityQuestName { get; set; }

		public virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.M) && !SteamPunkConsole.IsConsoleOpen)
			{
				TopPriorityQuest?.ForceComplete();
			}
		}

		public virtual void AcceptQuest(Quest quest)
		{
			SetQuestTaker(quest);

			if (!questLog.HasActiveQuest)
			{
				SetTopPriorityQuest(quest);
			}
			questLog.AddQuest(quest);
			ReceivedActiveQuest(quest);
		}

		private void SetQuestTaker(Quest quest)
		{
			quest.QuestTaker = this;
		}

		private void ReceivedActiveQuest(Quest quest)
		{
			OnQuestAccepted?.Invoke(quest);
			quest.OnQuestComplete += QuestCompleted;
		}

		private void QuestCompleted(Quest quest)
		{
			if (quest == TopPriorityQuest)
			{
				TopPriorityQuest = questLog.GetNextAvailableQuest();
				TopPriorityQuestName = TopPriorityQuest?.Name ?? string.Empty;
			}
			OnQuestCompleted?.Invoke(quest);
		}

		public void SetTopPriorityQuest(Quest quest)
		{
			TopPriorityQuest = quest;
			TopPriorityQuestName = TopPriorityQuest?.Name ?? string.Empty;
			OnTopPriorityQuestSet?.Invoke(quest);
		}

		public void ReceiveReward(object reward) => OnRewardReceived?.Invoke(reward);

		public bool IsNameOfCompletedQuest(string questName)
		{
			return questLog.CompletedListContains(t => t.Name == questName);
		}

		public bool IsNameOfActiveQuest(string questName)
		{
			return questLog.ActiveListContains(t => t.Name == questName);
		}

		private const string SAVE_TAG_NAME = "Quester",
			TOP_PRIORITY_QUEST_VAR_NAME = "Top Priority Quest";

		public void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save quest log
			questLog.Save(filename, mainTag);
			//save name of top priority quest
			DataModule module = new DataModule(TOP_PRIORITY_QUEST_VAR_NAME, TopPriorityQuest?.Name ?? string.Empty);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		public bool RecogniseTag(SaveTag tag)
		{
			return tag.TagName == SAVE_TAG_NAME;
		}

		public bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
				case TOP_PRIORITY_QUEST_VAR_NAME:
					TopPriorityQuestName = module.data;
					if (TopPriorityQuestName == string.Empty) break;

					if (IsNameOfActiveQuest(TopPriorityQuestName))
					{
						TopPriorityQuest = questLog.GetActiveQuestByName(TopPriorityQuestName);
					}
					else
					{
						questLog.OnActiveQuestAdded += WaitForQuestToBeAdded;

						void WaitForQuestToBeAdded(Quest quest)
						{
							if (quest.Name != TopPriorityQuestName) return;
							SetTopPriorityQuest(quest);
							questLog.OnActiveQuestAdded -= WaitForQuestToBeAdded;
						}
					}
					break;
			}

			return false;
		}

		public bool CheckSubtag(string filename, SaveTag subtag)
		{
			if (questLog.RecogniseTag(subtag))
			{
				UnifiedSaveLoad.IterateTagContents(
					filename,
					subtag,
					parameterCallBack: module => questLog.ApplyData(module),
					subtagCallBack: st => questLog.CheckSubtag(filename, st));

				Action<Quest> a = SetQuestTaker;
				questLog.IterateCompletedQuests(a);

				a += ReceivedActiveQuest;
				questLog.IterateActiveQuests(a);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}