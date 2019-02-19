using System.Collections.Generic;

public class QuestLog
{
	private List<Quest> activeQuests = new List<Quest>();
	private List<Quest> completedQuests = new List<Quest>();

	public void AddQuest(Quest quest)
	{
		if (quest.IsComplete())
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
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (activeQuests[i] == quest)
			{
				activeQuests.RemoveAt(i);
				completedQuests.Add(quest);
				return;
			}
		}
	}
}
