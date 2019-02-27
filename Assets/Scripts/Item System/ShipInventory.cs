using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInventory : Inventory
{
	public const string SHIP_INVENTORY_SIZE = "ShipInventorySize";
	public const string SHIP_SLOT_TYPE = "ShipSlot{0}Type";
	public const string SHIP_SLOT_AMOUNT = "ShipSlot{0}Amount";
	public SceneryController sceneryCtrl;

	protected override void Awake()
	{
		Load();
	}

	public void Save()
	{
		PlayerPrefs.SetInt(SHIP_INVENTORY_SIZE, size);
		for (int i = 0; i < size; i++)
		{
			PlayerPrefs.SetInt(string.Format(SHIP_SLOT_TYPE, i), (int)stacks[i].GetItemType());
			PlayerPrefs.SetInt(string.Format(SHIP_SLOT_AMOUNT, i), stacks[i].GetAmount());
		}
	}

	public void Load()
	{
		int savedSize = PlayerPrefs.GetInt(SHIP_INVENTORY_SIZE);
		size = savedSize != 0 ? savedSize : 50;
		base.Awake();

		for (int i = 0; i < savedSize; i++)
		{
			int type = PlayerPrefs.GetInt(string.Format(SHIP_SLOT_TYPE, i));
			int amount = PlayerPrefs.GetInt(string.Format(SHIP_SLOT_AMOUNT, i));
			stacks[i] = new ItemStack((Item.Type)type, amount);
		}
	}

	public void Store(List<ItemStack> items)
	{
		AddItems(items);
		Save();
		if (sceneryCtrl) sceneryCtrl.Save();
	}
}