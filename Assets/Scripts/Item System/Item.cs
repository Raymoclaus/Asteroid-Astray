using UnityEngine;

public static class Item
{
	public enum Type
	{
		Blank,
		Stone,
		Iron,
		Copper,
		PureCorvorite,
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
		NiolerDung,
		StoneAmmo
	}

	public static Sprite[] sprites;

	public const int MAX_RARITY = 10;

	public static string TypeName(Type type)
	{
		switch (type)
		{
			case Type.Blank: return "Blank";

			case Type.Stone: return "Stone";
			case Type.Iron: return "Iron";
			case Type.Copper: return "Copper";
			case Type.PureCorvorite: return "Pure Corvorite";
			case Type.CorruptedCorvorite: return "Corrupted Corvorite";
			case Type.WarpCoreBattery: return "Warp Core Battery";
			case Type.IronAlloy: return "Iron Alloy";
			case Type.BugFood: return "Bug Food";
			case Type.ProximityMine: return "Proximity Mine";
			case Type.UnstableAcid: return "Unstable Acid";
			case Type.EnergyDrink: return "Energy Drink";
			case Type.DataChip: return "Data Chip";
			case Type.HeatResistantIce: return "Heat Resistant Ice";
			case Type.Amber: return "Amber";
			case Type.Probe: return "Probe";
			case Type.Beacon: return "Beacon";
			case Type.ShieldGenerator: return "Shield Generator";
			case Type.SpareParts: return "Spare Parts";
			case Type.RepairKit: return "Repair Kit";
			case Type.NioleriumCrystals: return "Niolerium Crystals";
			case Type.NiolerDung: return "Nioler Dung";
			case Type.StoneAmmo: return "Stone Ammo";

			default: return "<Unnamed>";
		}
	}

	public static int TypeRarity(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;

			case Type.Stone: return 1;
			case Type.Iron: return 2;
			case Type.Copper: return 2;
			case Type.PureCorvorite: return 8;
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
			case Type.StoneAmmo: return 1;

			default: return 1;
		}
	}

	public static int StackLimit(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;

			case Type.Stone: return 100;
			case Type.Iron: return 100;
			case Type.Copper: return 100;
			case Type.PureCorvorite: return 5;
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
			case Type.StoneAmmo: return 1000;

			default: return 100;
		}
	}

	//limit to ~80 characters
	public static string ItemDescription(Type type)
	{
		switch (type)
		{
			case Type.Blank: return string.Empty;

			case Type.Stone: return "A lump of rock. Can be broken down for a chance to find" +
					" other items.";
			case Type.Iron: return "A refined metal. Can be used in crafting more complex items.";
			case Type.Copper: return "A refined metal. Can be used in crafting more complex" +
					" items.";
			case Type.PureCorvorite: return "A mysteriously rare mineral with uniquely powerful" +
					" properties.";
			case Type.CorruptedCorvorite:
				return "An impure source of energy resource derived from the legend known as" +
					" \"Pure Corvorite\".";
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
			case Type.StoneAmmo: return "A stone-based ammunition that is low-damage but is" +
					" useful for penetrating shields.";

			default: return string.Empty;
		}
	}

	public static string ItemFlavourText(Type type)
	{
		switch (type)
		{
			case Type.Blank: return string.Empty;

			case Type.Stone: return "\"Charged with two counts of murder by the Avian Court of" +
					" Caw Law.\"";
			case Type.Iron: return "\"Not quite as effective at removing creases from clothing" +
					" as you might think\"";
			case Type.Copper: return string.Empty;
			case Type.PureCorvorite: return "Pure Corvorite can be used as a hugely efficient" +
					" energy source however, it has only been found in trace amounts within" +
					" abandoned Gather Bot hives.";
			case Type.CorruptedCorvorite:
				return "Useful as an energy source however, due to the dangers involved with" +
					" obtaining it from fighting Gather Bots, it is too expensive compared to" +
					" more traditional fuel sources. It can be sold to researchers for a high" +
					" price. It is theorised that Gather Bot hives recharge their bots regularly" +
					" with only small amounts of energy from Pure Corvorite to minimise losses" +
					" if the Gather Bots are attacked.";
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
			case Type.StoneAmmo: return string.Empty;

			default: return string.Empty;
		}
	}
}