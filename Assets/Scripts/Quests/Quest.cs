using System.Collections.Generic;
using System.Linq;

public class Quest
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public IQuester Quester { get; private set; }
	public EntityProfile Issuer { get; private set; }
	public List<QuestReward> Rewards { get; private set; }
	public List<QuestRequirement> Requirements { get; private set; }

	public delegate void QuestCompleteEventHandler(Quest quest);
	public event QuestCompleteEventHandler OnQuestComplete;
	public void QuestComplete(Quest quest) => OnQuestComplete?.Invoke(quest);

	public Quest(string name, string description, IQuester quester, EntityProfile issuer,
		List<QuestReward> rewards, List<QuestRequirement> requirements)
	{
		Name = name;
		Description = description;
		Quester = quester;
		Issuer = issuer;
		Rewards = rewards;
		Requirements = requirements;

		foreach (QuestRequirement requirement in requirements)
		{
			requirement.OnQuestRequirementUpdated += EvaluateRequirements;
		}
	}

	private void EvaluateRequirements()
	{
		if (IsComplete())
		{
			QuestComplete(this);
			foreach (QuestReward reward in Rewards)
			{
				reward.GiveReward(Quester);
			}
		}
	}

	public bool IsComplete()
	{
		return Requirements.All(r => r.IsComplete());
	}
}
