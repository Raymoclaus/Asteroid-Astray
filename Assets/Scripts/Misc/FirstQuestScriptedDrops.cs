using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

public static class FirstQuestScriptedDrops
{
	public static bool scriptedDropsActive = false;
	private static List<List<ItemStack>> items = new List<List<ItemStack>>
	{
		new List<ItemStack>
		{
			new ItemStack(Item.Type.Iron, 1)
		},
		new List<ItemStack>
		{
			new ItemStack(Item.Type.Copper, 1)
		},
		new List<ItemStack>
		{
			new ItemStack(Item.Type.Copper, 1)
		},
		new List<ItemStack>(),
		new List<ItemStack>(),
		new List<ItemStack>()
	};

	public static List<ItemStack> GetScriptedDrop(IInventoryHolder target)
	{
		if (target is Shuttle) return null;
		if (items.Count == 0) scriptedDropsActive = false;
		if (!scriptedDropsActive) return null;

		int random = Random.Range(1, items.Count);
		if (random >= items.Count)
		{
			random = 0;
		}
		List<ItemStack> stacks = items[random];
		items.RemoveAt(random);
		scriptedDropsActive = items.Count != 0;
		return stacks;
	}
}
