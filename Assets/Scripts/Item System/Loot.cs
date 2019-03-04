using UnityEngine;

[System.Serializable]
public struct Loot
{
	public Item.Type type;
	public int maxAmount;
	public float lootChance;
	public double minAmountPercentage;
	public bool roundedUp;

	public Loot(Item.Type type = Item.Type.Stone, int maxAmount = 1, float lootChance = 0.5f, double minAmountPercentage = 0.5,
		bool roundedUp = true)
	{
		this.type = type;
		this.maxAmount = Mathf.Max(maxAmount, 1);
		this.lootChance = Mathf.Clamp01(lootChance);
		this.minAmountPercentage = Mathf.Clamp01((float)minAmountPercentage);
		this.roundedUp = roundedUp;
	}

	public ItemStack GetStack()
	{
		int minAmount;
		if (roundedUp)
		{
			minAmount = (int)System.Math.Ceiling(maxAmount * minAmountPercentage);
		}
		else
		{
			minAmount = (int)System.Math.Floor(maxAmount * minAmountPercentage);
		}

		int amount = minAmount;
		for (int i = minAmount; i < maxAmount; i++)
		{
			if (Random.value <= lootChance) amount++;
		}
		
		return new ItemStack(type, amount);
	}
}
