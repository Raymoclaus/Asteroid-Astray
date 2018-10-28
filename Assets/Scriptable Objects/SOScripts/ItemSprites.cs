using UnityEngine;

[CreateAssetMenu]
public class ItemSprites : ScriptableObject
{
	public Sprite
	Blank,
	PureCorvorite,
	Stone,
	CorruptedCorvorite,
	WarpCoreBattery,
	IronAlloy,
	BugFood,
	ProximityMine,
	UnstableAcid,
	EnergyDrink,
	DataChip,
	HeatResistantIce,
	Amber,
	Probe,
	Beacon,
	ShieldGenerator,
	SpareParts,
	RepairKit,
	NioleriumCrystals,
	NiolerDung;

	public Sprite GetItemSprite(Item.Type type)
	{
		switch (type)
		{
			default: return Blank;
			case Item.Type.Blank: return Blank;
			case Item.Type.PureCorvorite: return PureCorvorite;
			case Item.Type.Stone: return Stone;
			case Item.Type.CorruptedCorvorite: return CorruptedCorvorite;
			case Item.Type.WarpCoreBattery: return WarpCoreBattery;
			case Item.Type.IronAlloy: return IronAlloy;
			case Item.Type.BugFood: return BugFood;
			case Item.Type.ProximityMine: return ProximityMine;
			case Item.Type.UnstableAcid: return UnstableAcid;
			case Item.Type.EnergyDrink: return EnergyDrink;
			case Item.Type.DataChip: return DataChip;
			case Item.Type.HeatResistantIce: return HeatResistantIce;
			case Item.Type.Amber: return Amber;
			case Item.Type.Probe: return Probe;
			case Item.Type.Beacon: return Beacon;
			case Item.Type.ShieldGenerator: return ShieldGenerator;
			case Item.Type.SpareParts: return SpareParts;
			case Item.Type.RepairKit: return RepairKit;
			case Item.Type.NioleriumCrystals: return NioleriumCrystals;
			case Item.Type.NiolerDung: return NiolerDung;
		}
	}
}
