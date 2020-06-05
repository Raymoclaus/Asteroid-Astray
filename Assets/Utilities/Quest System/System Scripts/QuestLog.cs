using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestSystem
{
	public class QuestLog
	{
		private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
		private Dictionary<string, Quest> completedQuests = new Dictionary<string, Quest>();

		private Quester _quester;

		//these are also triggered when the game loads
		public event Action<Quest> OnActiveQuestAdded, OnCompletedQuestAdded;

		public void AddQuest(Quest quest)
		{
			if (quest == null) return;

			if (quest.IsComplete)
			{
				completedQuests.Add(quest.Name, quest);
				OnCompletedQuestAdded?.Invoke(quest);
			}
			else
			{
				quest.OnQuestComplete += QuestIsCompleted;
				activeQuests.Add(quest.Name, quest);
				quest.Activate();
				OnActiveQuestAdded?.Invoke(quest);
			}
		}

		public bool HasActiveQuest
			=> activeQuests.Count > 0;

		private void QuestIsCompleted(Quest quest)
		{
			if (!activeQuests.Remove(quest.Name)) return;
			completedQuests.Add(quest.Name, quest);
			OnCompletedQuestAdded?.Invoke(quest);
		}

		public Quest GetNextAvailableQuest()
			=> activeQuests.Values.FirstOrDefault(t => !t.IsComplete);

		public Quest GetActiveQuestByName(string questName)
		{
			if (activeQuests.ContainsKey(questName)) return activeQuests[questName];
			return null;
		}

		public Quest GetCompletedQuestByName(string questName)
		{
			if (completedQuests.ContainsKey(questName)) return completedQuests[questName];
			return null;
		}

		public bool CompletedListContains(Func<Quest, bool> predicate)
		{
			return completedQuests.Values.FirstOrDefault(predicate) != null;
		}

		public bool ActiveListContains(Func<Quest, bool> predicate)
		{
			return activeQuests.Values.FirstOrDefault(predicate) != null;
		}

		public void IterateActiveQuests(Action<Quest> action)
		{
			if (action == null) return;

			foreach (Quest q in activeQuests.Values)
			{
				action(q);
			}
		}

		public void IterateCompletedQuests(Action<Quest> action)
		{
			if (action == null) return;

			foreach (Quest q in completedQuests.Values)
			{
				action(q);
			}
		}

		private const string SAVE_TAG_NAME = "Quest Log";

		public void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//iterate over all completed quests
			foreach (Quest q in completedQuests.Values)
			{
				q.Save(filename, mainTag);
			}
			//iterate over all active quests
			foreach (Quest q in activeQuests.Values)
			{
				q.Save(filename, mainTag);
			}
		}

		public bool RecogniseTag(SaveTag tag)
		{
			return tag.TagName == SAVE_TAG_NAME;
		}

		public bool ApplyData(DataModule module)
		{
			return false;
		}

		public bool CheckSubtag(string filename, SaveTag subtag)
		{
			if (Quest.RecogniseTag(subtag))
			{
				Quest q = Quest.LoadQuestFromFile(filename, subtag);
				AddQuest(q);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}