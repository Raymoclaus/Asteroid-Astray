﻿using System.Linq;
using System.Collections.Generic;
using SaveSystem;

namespace QuestSystem
{
	public class QuestLog
	{
		private List<Quest> activeQuests = new List<Quest>();
		private List<Quest> completedQuests = new List<Quest>();

		public bool HasActiveQuest
			=> activeQuests.Exists(t => !t.IsComplete);

		public void AddQuest(Quest quest)
		{
			if (quest.IsComplete)
			{
				completedQuests.Add(quest);
			}
			else
			{
				quest.OnQuestComplete += QuestIsCompleted;
				activeQuests.Add(quest);
			}
		}

		private void QuestIsCompleted(Quest quest)
		{
			if (!activeQuests.Remove(quest)) return;
			completedQuests.Add(quest);
		}

		public Quest GetNextAvailableQuest()
			=> activeQuests.FirstOrDefault(t => !t.IsComplete);

		private const string SAVE_TAG_NAME = "Quest Log";
		public void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//iterate over all completed quests
			foreach (Quest q in completedQuests)
			{
				q.Save(mainTag);
			}
			//iterate over all active quests
			foreach (Quest q in activeQuests)
			{
				q.Save(mainTag);
			}
		}
	}

}