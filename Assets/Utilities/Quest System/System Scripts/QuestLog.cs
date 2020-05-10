using System.Linq;
using System.Collections.Generic;
using SaveSystem;

namespace QuestSystem
{
	public class QuestLog
	{
		private List<Quest> activeQuests = new List<Quest>();
		private List<Quest> completedQuests = new List<Quest>();

		public bool HasActiveQuest
			=> activeQuests.Count > 0;

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

		public bool CompletedListContains(System.Func<Quest, bool> predicate)
		{
			return completedQuests.FirstOrDefault(predicate) != null;
		}

		public bool ActiveListContains(System.Func<Quest, bool> predicate)
		{
			return activeQuests.FirstOrDefault(predicate) != null;
		}

		private const string SAVE_TAG_NAME = "Quest Log";

		public void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//iterate over all completed quests
			foreach (Quest q in completedQuests)
			{
				q.Save(filename, mainTag);
			}
			//iterate over all active quests
			foreach (Quest q in activeQuests)
			{
				q.Save(filename, mainTag);
			}
		}
	}

}