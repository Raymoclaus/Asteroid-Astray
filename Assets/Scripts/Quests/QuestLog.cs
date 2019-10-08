using System.Linq;
using System.Collections.Generic;

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
}
