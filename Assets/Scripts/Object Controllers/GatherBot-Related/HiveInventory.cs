using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HiveInventory : Inventory
{
	public void Store(List<ItemStack> items)
	{
		do
		{
			AddItems(items);
			ConvertResources();
		} while (CheckLeftovers(items));
	}

	private void ConvertResources()
	{
		int[] rarityCounts = CountRarities(Item.Type.Corvorite);

		for (int i = 1; i < rarityCounts.Length; i++)
		{
			int count = rarityCounts[i];
			Vector2Int costReturn = rarityCostReturns(i);
			if (count >= costReturn.x)
			{
				int amount = count / costReturn.x;
				AddItem(Item.Type.Corvorite, amount);
				RemoveByRarity(i, amount * count, Item.Type.Corvorite);
			}
		}
	}

	private Vector2Int rarityCostReturns(int rarity)
	{
		Vector2Int costReturn = Vector2Int.one;
		switch (rarity)
		{
			case 0:
				costReturn.x = int.MaxValue;
				break;
			case 1:
				costReturn.x = 100;
				break;
			case 2:
				costReturn.x = 50;
				break;
			case 3:
				costReturn.x = 10;
				break;
			case 4:
				costReturn.x = 5;
				break;
			case 5:
				costReturn.x = 2;
				break;
			case 6:
				costReturn.y = 2;
				break;
			case 7:
				costReturn.y = 3;
				break;
			case 8:
				costReturn.y = 5;
				break;
			case 9:
				costReturn.y = 7;
				break;
			case 10:
				costReturn.y = 10;
				break;
		}

		return costReturn;
	}

	private bool CheckLeftovers(List<ItemStack> leftovers)
	{
		foreach (ItemStack stack in leftovers)
		{
			if (stack.GetAmount() > 0) return true;
		}
		return false;
	}
}