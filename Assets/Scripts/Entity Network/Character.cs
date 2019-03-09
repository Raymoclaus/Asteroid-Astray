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

	protected bool UseItem(Item.Type type)
	{
		switch (type)
		{
			case Item.Type.Blank:
				break;
			case Item.Type.Stone:
				break;
			case Item.Type.Iron:
				break;
			case Item.Type.Copper:
				break;
			case Item.Type.PureCorvorite:
				break;
			case Item.Type.CorruptedCorvorite:
				break;
			case Item.Type.WarpCoreBattery:
				break;
			case Item.Type.IronAlloy:
				break;
			case Item.Type.BugFood:
				break;
			case Item.Type.ProximityMine:
				break;
			case Item.Type.UnstableAcid:
				break;
			case Item.Type.EnergyDrink:
				break;
			case Item.Type.DataChip:
				break;
			case Item.Type.HeatResistantIce:
				break;
			case Item.Type.Amber:
				break;
			case Item.Type.Probe:
				break;
			case Item.Type.Beacon:
				break;
			case Item.Type.ShieldGenerator:
				break;
			case Item.Type.SpareParts:
				break;
			case Item.Type.RepairKit:
				return true;
			case Item.Type.NioleriumCrystals:
				break;
			case Item.Type.NiolerDung:
				break;
			case Item.Type.StoneAmmo:
				break;
		}
		return false;
	}
}