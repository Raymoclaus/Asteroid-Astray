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
	}
}