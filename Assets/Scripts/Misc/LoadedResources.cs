using UnityEngine;

public class LoadedResources : MonoBehaviour
{
	//large, medium, small asteroids
	public NestedSpriteArray[] asteroidSprites;

	//asteroid debris
	public Sprite[] debris;

	//asteroid dust
	public Sprite[] dust;

	//item sprites
	public ItemSprites itemSprites;

	public Sprite GetItemSprite(Item.Type type)
	{
		switch (type)
		{
			default: return itemSprites.Blank;
			case Item.Type.Blank: return itemSprites.Blank;
			case Item.Type.PureCorvorite: return itemSprites.PureCorvorite;
			case Item.Type.Stone: return itemSprites.Stone;
			case Item.Type.CorruptedCorvorite: return itemSprites.CorruptedCorvorite;
			case Item.Type.WarpCoreBattery: return itemSprites.WarpCoreBattery;
			case Item.Type.IronAlloy: return itemSprites.IronAlloy;
			case Item.Type.BugFood: return itemSprites.BugFood;
			case Item.Type.ProximityMine: return itemSprites.ProximityMine;
			case Item.Type.UnstableAcid: return itemSprites.UnstableAcid;
			case Item.Type.EnergyDrink: return itemSprites.EnergyDrink;
			case Item.Type.DataChip: return itemSprites.DataChip;
			case Item.Type.HeatResistantIce: return itemSprites.HeatResistantIce;
			case Item.Type.Amber: return itemSprites.Amber;
			case Item.Type.Probe: return itemSprites.Probe;
			case Item.Type.Beacon: return itemSprites.Beacon;
			case Item.Type.ShieldGenerator: return itemSprites.ShieldGenerator;
			case Item.Type.SpareParts: return itemSprites.SpareParts;
			case Item.Type.RepairKit: return itemSprites.RepairKit;
			case Item.Type.NioleriumCrystals: return itemSprites.NioleriumCrystals;
			case Item.Type.NiolerDung: return itemSprites.NiolerDung;
		}
	}

	[System.Serializable]
	public class ItemSprites
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
	}
}

[System.Serializable]
public struct SpriteArray
{
	//different crack levels
	public Sprite[] sprites;
}

[System.Serializable]
public struct NestedSpriteArray
{
	//different shapes
	public SpriteArray[] collection;
}