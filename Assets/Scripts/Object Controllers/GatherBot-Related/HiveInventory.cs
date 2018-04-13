using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		int[] rarityCounts = CountRarities();

		for (int i = 1; i < rarityCounts.Length; i++)
		{
			int count = rarityCounts[i];
			Vector2Int costReturn = rarityCostReturns(i);
			if (count >= costReturn.x)
			{
				int amount = count / costReturn.x * count;
				count -= amount;
				RemoveByRarity(i, amount);
			}
		}
	}

	private Vector2Int rarityCostReturns(int rarity)
	{
		Vector2Int costReturn = Vector2Int.one;
		switch (rarity)
		{
			case 0:
				costReturn.y = 0;
				break;
			case 1:
				costReturn.y = 100;
				break;
			case 2:
				costReturn.y = 50;
				break;
			case 3:
				costReturn.y = 10;
				break;
			case 4:
				costReturn.y = 5;
				break;
			case 5:
				costReturn.y = 2;
				break;
			case 6:
				costReturn.x = 2;
				break;
			case 7:
				costReturn.x = 3;
				break;
			case 8:
				costReturn.x = 5;
				break;
			case 9:
				costReturn.x = 7;
				break;
			case 10:
				costReturn.x = 10;
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