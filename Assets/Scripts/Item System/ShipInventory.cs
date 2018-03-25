﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInventory : Inventory
{
	public static ShipInventory singleton;
	public const string SHIP_INVENTORY_SIZE = "ShipInventorySize";
	public const string SHIP_SLOT_TYPE = "ShipSlot{0}Type";
	public const string SHIP_SLOT_AMOUNT = "ShipSlot{0}Amount";

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		Load();
	}

	public void Save()
	{
		PlayerPrefs.GetInt(SHIP_INVENTORY_SIZE, size);
		for (int i = 0; i < size; i++)
		{
			PlayerPrefs.SetInt(string.Format(SHIP_SLOT_TYPE, i), (int)inventory[i].GetItemType());
			PlayerPrefs.SetInt(string.Format(SHIP_SLOT_AMOUNT, i), inventory[i].GetAmount());
		}
	}

	public void Load()
	{
		size = PlayerPrefs.GetInt(SHIP_INVENTORY_SIZE);

		for (int i = 0; i < size; i++)
		{
			int type = PlayerPrefs.GetInt(string.Format(SHIP_SLOT_TYPE, i));
			int amount = PlayerPrefs.GetInt(string.Format(SHIP_SLOT_AMOUNT, i));
			inventory[i] = new ItemStack((Item.Type)type, amount);
		}
	}
}