using System;
using System.Collections.Generic;

namespace InventorySystem
{
	[Serializable]
	public struct LootGroup
	{
		public List<Loot> group;

		public int Count => group?.Count ?? 0;

		public bool IsEmpty => Count == 0;

		public List<ItemStack> GetStacks
		{
			get
			{
				List<ItemStack> stacks = new List<ItemStack>();

				for (int i = 0; i < Count; i++)
				{
					ItemStack stack = group[i].GetStack();
					stacks.Add(stack);
				}

				return stacks;
			}
		}
	} 
}
