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
		List<QuestReward> rewards, List<QuestRequirement> requirements, QuestCompleteEventHandler action = null)
	{
		Name = name;
		Description = description;
		Quester = quester;
		Issuer = issuer;
		Rewards = rewards;
		Requirements = requirements;
		OnQuestComplete += action;

		for (int i = 0; i < Requirements.Count; i++)
		{
			QuestRequirement requirement = Requirements[i];
			requirement.OnQuestRequirementCompleted += EvaluateRequirements;
		}
	}

	private void EvaluateRequirements()
	{
		if (!IsComplete) return;

		Debug.Log($"Quest Complete: {Name}");
		QuestComplete(this);
		Rewards.ForEach(t => t.GiveReward(Quester));
	}

	public bool IsComplete => !Requirements.Exists(t => !t.Completed);

	public void Activate() => Requirements.ForEach(t => t.Activate());

	public void ForceComplete()
		=> Requirements.ForEach(t => t.QuestRequirementCompleted());
}
