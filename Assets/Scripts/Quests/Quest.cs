using System.Collections.Generic;
using UnityEngine;

public class Quest
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public Character Quester { get; private set; }
	public EntityProfile Issuer { get; private set; }
	public List<QuestReward> Rewards { get; private set; }
	public List<QuestRequirement> Requirements { get; private set; }

	public delegate void QuestCompleteEventHandler(Quest quest);
	public event QuestCompleteEventHandler OnQuestComplete;
	public void QuestComplete(Quest quest) => OnQuestComplete?.Invoke(quest);

	public Quest(string name, string description, Character quester, EntityProfile issuer,
		List<QuestReward> rewards, List<QuestRequirement> requirements)
	{
		Name = name;
		Description = description;
		Quester = quester;
		Issuer = issuer;
		Rewards = rewards;
		Requirements = requirements;

		for (int i = 0; i < Requirements.Count; i++)
		{
			QuestRequirement requirement = Requirements[i];
			requirement.OnQuestRequirementUpdated += EvaluateRequirements;
		}
	}

	private void EvaluateRequirements()
	{
		if (IsComplete())
		{
			Debug.Log($"Quest Complete: {Name}");
			QuestComplete(this);
			for (int i = 0; i < Rewards.Count; i++)
			{
				QuestReward reward = Rewards[i];
				reward.GiveReward(Quester);
			}
		}
	}

	public bool IsComplete()
	{
		for (int i = 0; i < Requirements.Count; i++)
		{
			if (!Requirements[i].IsComplete()) return false;
		}
		return true;
	}

	public void Activate()
	{
		for (int i = 0; i < Requirements.Count; i++)
		{
			QuestRequirement requirement = Requirements[i];
			requirement.Activate();
		}
	}
}
