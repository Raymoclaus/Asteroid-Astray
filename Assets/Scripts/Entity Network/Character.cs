public class Character : Entity
{
	private static QuestPopupUI questPopupUI;

	public virtual void ReceiveItemReward(Item.Type type, int amount)
	{
		CollectResources(type, amount);
	}

	public virtual void AcceptQuest(Quest quest)
	{
		quest.Activate();

		questPopupUI = questPopupUI ?? FindObjectOfType<QuestPopupUI>();
		questPopupUI?.GeneratePopup(quest);
	}
}
