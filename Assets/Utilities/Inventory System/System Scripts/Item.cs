namespace InventorySystem
{
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
			CoreCrystal,
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
			StoneAmmo,
			CureShotAmmo,
			BlueKey,
			RedKey,
			YellowKey,
			GreenKey,
		}

		public const int MAX_RARITY = 10;

		public const int MIN_RARITY = 0;

		public static string TypeName(Type type)
		{
			switch (type)
			{
				default: return "<Unnamed>";
				case Type.Blank: return "Blank";
				case Type.Stone: return "Stone";
				case Type.Iron: return "Iron";
				case Type.Copper: return "Copper";
				case Type.PureCorvorite: return "Pure Corvorite";
				case Type.CorruptedCorvorite: return "Corrupted Corvorite";
				case Type.CoreCrystal: return "Core Crystal";
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
				case Type.CureShotAmmo: return "CureShot Ammo";
				case Type.BlueKey: return "Blue Key";
				case Type.RedKey: return "Red Key";
				case Type.YellowKey: return "Yellow Key";
				case Type.GreenKey: return "Green Key";
			}
		}

		public static int TypeRarity(Type type)
		{
			switch (type)
			{
				default: return 1;
				case Type.Blank: return 0;
				case Type.Stone: return 1;
				case Type.Iron: return 2;
				case Type.Copper: return 2;
				case Type.PureCorvorite: return 8;
				case Type.CorruptedCorvorite: return 6;
				case Type.CoreCrystal: return 5;
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
				case Type.StoneAmmo: return 2;
				case Type.CureShotAmmo: return 3;
				case Type.BlueKey: return 1;
				case Type.RedKey: return 1;
				case Type.YellowKey: return 1;
				case Type.GreenKey: return 1;
			}
		}

		public static int StackLimit(Type type)
		{
			switch (type)
			{
				default: return 100;
				case Type.Blank: return 0;
				case Type.Stone: return 100;
				case Type.Iron: return 100;
				case Type.Copper: return 100;
				case Type.PureCorvorite: return 5;
				case Type.CorruptedCorvorite: return 30;
				case Type.CoreCrystal: return 1;
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
				case Type.StoneAmmo: return 100;
				case Type.CureShotAmmo: return 100;
				case Type.BlueKey: return 1;
				case Type.RedKey: return 1;
				case Type.YellowKey: return 1;
				case Type.GreenKey: return 1;
			}
		}

		public static bool IsKeyItem(Type type)
		{
			switch (type)
			{
				default: return false;
				case Type.BlueKey: return true;
				case Type.RedKey: return true;
				case Type.YellowKey: return true;
				case Type.GreenKey: return true;
			}
		}

		//limit to ~80 characters
		public static string Description(Type type)
		{
			switch (type)
			{
				default: return string.Empty;
				case Type.Blank: return string.Empty;
				case Type.Stone:
					return "A lump of rock. Can be broken down for a chance to find" +
						" other items.";
				case Type.Iron: return "A refined metal. Can be used in crafting" +
						" more complex items.";
				case Type.Copper:
					return "A refined metal. Can be used in crafting more complex" +
						" items.";
				case Type.PureCorvorite:
					return "A mysteriously rare mineral with uniquely powerful" +
						" properties.";
				case Type.CorruptedCorvorite:
					return "An impure source of energy resource derived" +
						" from the legend known as \"Pure Corvorite\".";
				case Type.CoreCrystal: return string.Empty;
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
				case Type.StoneAmmo:
					return "Stone-based ammunition that is low-damage but is useful" +
						" for penetrating shields.";
				case Type.CureShotAmmo:
					return "Ammunition for injecting nanobots into the target" +
						" and repair mechanical systems.";
				case Type.BlueKey:
					return "A blue key. Found on any planet, but mysteriously" +
						" crumble on leaving a planet.";
				case Type.RedKey:
					return "A red key. Found on any planet, but mysteriously" +
						" crumble on leaving a planet.";
				case Type.YellowKey:
					return "A yellow key. Found on any planet, but mysteriously" +
						" crumble on leaving a planet.";
				case Type.GreenKey:
					return "A green key. Found on any planet, but mysteriously" +
						" crumble on leaving a planet.";
			}
		}

		public static string FlavourText(Type type)
		{
			switch (type)
			{
				default: return string.Empty;
				case Type.Blank: return string.Empty;
				case Type.Stone:
					return "\"Charged with two counts of murder by the Avian Court" +
						" of Law.\"";
				case Type.Iron:
					return "\"Not quite as effective at removing creases from" +
						" clothing as you might think\"";
				case Type.Copper: return string.Empty;
				case Type.PureCorvorite:
					return "Pure Corvorite can be used as a hugely efficient" +
						" energy source however, it has only been found in trace" +
						" amounts within abandoned Gather Bot hives.";
				case Type.CorruptedCorvorite:
					return "Useful as an energy source however, due to the dangers" +
						" involved with obtaining it from fighting Gather Bots, it" +
						" is too expensive compared to more traditional fuel" +
						" sources. It can be sold to researchers for a high price." +
						" It is theorised that Gather Bot hives recharge their" +
						" bots regularly with only small amounts of energy from" +
						" Pure Corvorite to minimise losses if the Gather Bots are" +
						" attacked.";
				case Type.CoreCrystal: return string.Empty;
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
				case Type.StoneAmmo:
					return "Due to the special energy-piercing properties of the" +
						" stone found in asteroids of Lost space, it doesn't take" +
						" anyone long to figure out that it makes for useful ammo.";
				case Type.CureShotAmmo:
					return "Originally designed as a joke and was often used as" +
						" a form of disrespect by those capable of overpowering" +
						" others with ease. They would heal their prey to give a" +
						" false sense of mercy before finishing them off. CureShot" +
						" ammunition later saw some more practical use between" +
						" squads, but hasn't seen much more use due to its" +
						" difficulty to use in combat and has few other" +
						" applications.";
				case Type.BlueKey:
					return "These curious keys appear to only be able to exist in" +
						" one piece while on a planet. They have a strange reaction" +
						" with certain objects on their respective planets, causing" +
						" both the key and the object to disappear. Most believe" +
						" them to be magical items of tricksters, but no such" +
						" \"trickster\" has been sighted, nor does magic exist.";
				case Type.RedKey: goto case Type.BlueKey;
				case Type.YellowKey: goto case Type.BlueKey;
				case Type.GreenKey: goto case Type.BlueKey;
			}
		}
	}
}