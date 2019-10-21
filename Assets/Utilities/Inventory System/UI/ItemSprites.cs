using UnityEngine;

namespace InventorySystem.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/ItemSprites")]
	public class ItemSprites : ScriptableObject
	{
		public Sprite
			Blank,
			Stone,
			Iron,
			Copper,
			PureCorvorite,
			CorruptedCorvorite,
			CoreCrystal,
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
			StoneAmmo,
			CureShotAmmo,
			BlueKey,
			RedKey,
			YellowKey,
			GreenKey;

		public Sprite GetItemSprite(Item.Type type)
		{
			switch (type)
			{
				default: return Blank;
				case Item.Type.Blank: return Blank;
				case Item.Type.Stone: return Stone;
				case Item.Type.Iron: return Iron;
				case Item.Type.Copper: return Copper;
				case Item.Type.PureCorvorite: return PureCorvorite;
				case Item.Type.CorruptedCorvorite: return CorruptedCorvorite;
				case Item.Type.CoreCrystal: return CoreCrystal;
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
				case Item.Type.StoneAmmo: return StoneAmmo;
				case Item.Type.CureShotAmmo: return CureShotAmmo;
				case Item.Type.BlueKey: return BlueKey;
				case Item.Type.RedKey: return RedKey;
				case Item.Type.YellowKey: return YellowKey;
				case Item.Type.GreenKey: return GreenKey;
			}
		}
	}
}
