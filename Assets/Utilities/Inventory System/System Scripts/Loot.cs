﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InventorySystem
{
	[Serializable]
	public struct Loot
	{
		public ItemObject type;
		public int minAmount, maxAmount;
		public float lootChance;

		public Loot(ItemObject type, int minAmount, int maxAmount, float lootChance)
		{
			this.type = type;
			this.minAmount = Mathf.Min(minAmount, maxAmount);
			this.maxAmount = Mathf.Max(minAmount, maxAmount);
			this.lootChance = Mathf.Clamp01(lootChance);
		}

		public ItemStack GetStack()
		{
			int amount = minAmount;
			int potentialExtraLoot = maxAmount - minAmount;
			for (int i = 0; i < potentialExtraLoot; i++)
			{
				if (Random.value <= lootChance) amount++;
			}

			return new ItemStack(type, amount);
		}
	}

}