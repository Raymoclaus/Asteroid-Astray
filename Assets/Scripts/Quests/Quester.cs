using System;
using UnityEngine;

public class Quester : MonoBehaviour
{
	private QuestLog questLog = new QuestLog();
	public Action<Quest> questAccepted, questCompleted, topPriorityQuestSet;
	public Quest TopPriorityQuest { get; set; }
	
	public virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
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
		questAccepted?.Invoke(quest);
		quest.OnQuestComplete += QuestCompleted;
	}

	private void QuestCompleted(Quest quest)
	{
		if (quest == TopPriorityQuest)
		{
			TopPriorityQuest = questLog.GetNextAvailableQuest();
		}
		questCompleted?.Invoke(quest);
	}

	public void SetTopPriorityQuest(Quest quest)
	{
		TopPriorityQuest = quest;
		topPriorityQuestSet?.Invoke(quest);
	}
}
