using UnityEngine;

public static class Item
{
	public enum Type
	{
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
		NiolerDung
	}

	public static Sprite[] sprites;

	public const int MAX_RARITY = 10;

	public static int TypeRarity(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.PureCorvorite: return 8;
			case Type.Stone: return 1;
			case Type.CorruptedCorvorite: return 6;
			case Type.WarpCoreBattery: return 5;
			case Type.IronAlloy: return 2;
			case Type.BugFood: return 1;
			case Type.ProximityMine: return 3;
			case Type.UnstableAcid: return 2;
			case Type.EnergyDrink: return 1;
			case Type.DataChip: return 2;
			case Type.HeatResistantIce: return 4;
			case Type.Amber: return 3;
			case Type.Probe: return 5;
			case Type.Beacon: return 5;
			case Type.ShieldGenerator: return 4;
			case Type.SpareParts: return 2;
			case Type.RepairKit: return 3;
			case Type.NioleriumCrystals: return 3;
			case Type.NiolerDung: return 6;
			default: return 1;
		}
	}

	public static int StackLimit(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.PureCorvorite: return 5;
			case Type.Stone: return 100;
			case Type.CorruptedCorvorite: return 30;
			case Type.WarpCoreBattery: return 1;
			case Type.IronAlloy: return 100;
			case Type.BugFood: return 50;
			case Type.ProximityMine: return 10;
			case Type.UnstableAcid: return 30;
			case Type.EnergyDrink: return 50;
			case Type.DataChip: return 50;
			case Type.HeatResistantIce: return 30;
			case Type.Amber: return 50;
			case Type.Probe: return 1;
			case Type.Beacon: return 1;
			case Type.ShieldGenerator: return 1;
			case Type.SpareParts: return 100;
			case Type.RepairKit: return 10;
			case Type.NioleriumCrystals: return 30;
			case Type.NiolerDung: return 30;
			default: return 1;
		}
	}

	public static string ItemDescription(Type type)
	{
		switch (type)
		{
			case Type.Blank: return string.Empty;
			case Type.PureCorvorite: return string.Empty;
			case Type.Stone: return "A lump of rock. Can be used in crafting.";
			case Type.CorruptedCorvorite: return string.Empty;
			case Type.WarpCoreBattery: return string.Empty;
			case Type.IronAlloy: return string.Empty;
			case Type.BugFood: return string.Empty;
			case Type.ProximityMine: return string.Empty;
			case Type.UnstableAcid: return string.Empty;
			case Type.EnergyDrink: return string.Empty;
			case Type.DataChip: return string.Empty;
			case Type.HeatResistantIce: return string.Empty;
			case Type.Amber: return string.Empty;
			case Type.Probe: return string.Empty;
			case Type.Beacon: return string.Empty;
			case Type.ShieldGenerator: return string.Empty;
			case Type.SpareParts: return string.Empty;
			case Type.RepairKit: return string.Empty;
			case Type.NioleriumCrystals: return string.Empty;
			case Type.NiolerDung: return string.Empty;
			default: return string.Empty;
		}
	}

	public static string ItemFlavourText(Type type)
	{
		switch (type)
		{
			case Type.Blank: return string.Empty;
			case Type.PureCorvorite: return string.Empty;
			case Type.Stone: return "\"Charged with two counts of murder by the Avian Court of Caw Law.\"";
			case Type.CorruptedCorvorite: return string.Empty;
			case Type.WarpCoreBattery: return string.Empty;
			case Type.IronAlloy: return string.Empty;
			case Type.BugFood: return string.Empty;
			case Type.ProximityMine: return string.Empty;
			case Type.UnstableAcid: return string.Empty;
			case Type.EnergyDrink: return string.Empty;
			case Type.DataChip: return string.Empty;
			case Type.HeatResistantIce: return string.Empty;
			case Type.Amber: return string.Empty;
			case Type.Probe: return string.Empty;
			case Type.Beacon: return string.Empty;
			case Type.ShieldGenerator: return string.Empty;
			case Type.SpareParts: return string.Empty;
			case Type.RepairKit: return string.Empty;
			case Type.NioleriumCrystals: return string.Empty;
			case Type.NiolerDung: return string.Empty;
			default: return string.Empty;
		}
	}
}