using System;
using SaveSystem;
using UnityEngine;

namespace QuestSystem
{
	public class Quester : MonoBehaviour
	{
		private QuestLog questLog = new QuestLog();
		public event Action<Quest> OnQuestAccepted, OnQuestCompleted, OnTopPriorityQuestSet;
		public event Action<object> OnRewardReceived;
		public Quest TopPriorityQuest { get; set; }

		public virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.M) && !SteamPunkConsole.IsConsoleOpen)
			{
				TopPriorityQuest?.ForceComplete();
			}
		}

		public virtual void AcceptQuest(Quest quest)
		{
			quest.Activate();
			if (!questLog.HasActiveQuest)
			{
				SetTopPriorityQuest(quest);
			}
			questLog.AddQuest(quest);
			OnQuestAccepted?.Invoke(quest);
			quest.OnQuestComplete += QuestCompleted;
		}

		private void QuestCompleted(Quest quest)
		{
			if (quest == TopPriorityQuest)
			{
				TopPriorityQuest = questLog.GetNextAvailableQuest();
			}
			OnQuestCompleted?.Invoke(quest);
		}

		public void SetTopPriorityQuest(Quest quest)
		{
			TopPriorityQuest = quest;
			OnTopPriorityQuestSet?.Invoke(quest);
		}

		public void ReceiveReward(object reward) => OnRewardReceived?.Invoke(reward);

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

		public bool IsNameOfCompletedQuest(string questName)
		{
			return questLog.CompletedListContains(t => t.Name == questName);
		}

		public bool IsNameOfActiveQuest(string questName)
		{
			return questLog.ActiveListContains(t => t.Name == questName);
		}
	}
}