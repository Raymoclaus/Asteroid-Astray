using UnityEngine;

public class Character : Entity
{
	public delegate void ItemUsedEventHandler(Item.Type type);
	public event ItemUsedEventHandler OnItemUsed;
	public Inventory storage;

	#region Drill-related
	protected bool canDrill, canDrillLaunch;
	[SerializeField] protected DrillBit drill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	public DrillBit GetDrill() => canDrill ? drill : null;

	public void AttachDrill(DrillBit db) => drill = db;

	public virtual bool CanDrillLaunch() => canDrillLaunch;
	public virtual bool CanDrill() => canDrill;

	public virtual float GetLaunchDamage() => 0f;

	public virtual Vector2 LaunchDirection(Transform launchableObject) => Vector2.zero;

	public virtual bool ShouldLaunch() => false;

	//This should be overridden. Called by a drill to alert the entity that the drilling has completed
	public virtual void DrillComplete() { }

	//some entities might want to avoid drilling other entities by accident, override to verify target
	public virtual bool VerifyDrillTarget(Entity target) => true;

	//This should be overridden. Called by a drill to determine how much damage it should deal to its target.
	public virtual float DrillDamageQuery(bool firstHit) => 1f;

	public virtual float MaxDrillDamage() => 1f;

	public virtual void StoppedDrilling(bool successful) { }
	#endregion Drill-related

	public override ICombat GetICombat() => null;

	public virtual void ReceiveItemReward(Item.Type type, int amount) => CollectResources(type, amount);

	public virtual void AcceptQuest(Quest quest)
	{
		quest.Activate();
		QuestPopupUI.ShowQuest(quest);
	}

	protected bool UseItem(Item.Type type)
	{
		bool used = false;
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
			case Item.Type.CoreCrystal:
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
				used = true;
				break;
			case Item.Type.NioleriumCrystals:
				break;
			case Item.Type.NiolerDung:
				break;
			case Item.Type.StoneAmmo:
				break;
		}
		if (used)
		{
			OnItemUsed?.Invoke(type);
		}
		return used;
	}

	public virtual bool TakeItem(Item.Type type, int amount) => false;
}