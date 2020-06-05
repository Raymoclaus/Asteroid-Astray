using SaveSystem;
using System;
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
			this.lootChance = lootChance;
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

		private const string SAVE_TAG_NAME = "Loot",
			TYPE_VAR_NAME = "Item Type",
			MINIMUM_AMOUNT_VAR_NAME = "Minimum Amount",
			MAXIMUM_AMOUNT_VAR_NAME = "Maximum Amount",
			LOOT_CHANCE_VAR_NAME = "Loot Chance";

		public void Save(string filename, SaveTag parentTag, string modifier)
		{
			//create main tag
			SaveTag mainTag = new SaveTag($"{SAVE_TAG_NAME}:{modifier}", parentTag);
			//save item type
			DataModule module = new DataModule(TYPE_VAR_NAME, type.GetTypeName());
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save minimum amount
			module = new DataModule(MINIMUM_AMOUNT_VAR_NAME, minAmount);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save maximum amount
			module = new DataModule(MAXIMUM_AMOUNT_VAR_NAME, maxAmount);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save loot chance
			module = new DataModule(LOOT_CHANCE_VAR_NAME, lootChance);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		public static bool RecogniseTag(SaveTag tag) => tag.TagName.StartsWith(SAVE_TAG_NAME);

		public bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
				case TYPE_VAR_NAME:
				{
					type = Item.GetItemByName(module.data);
					break;
				}
				case MINIMUM_AMOUNT_VAR_NAME:
				{
					bool foundVal = int.TryParse(module.data, out int val);
					if (foundVal)
					{
						minAmount = val;
					}
					else
					{
						Debug.Log("Minimum Amount data could not be parsed.");
					}

					break;
				}
				case MAXIMUM_AMOUNT_VAR_NAME:
				{
					bool foundVal = int.TryParse(module.data, out int val);
					if (foundVal)
					{
						maxAmount = val;
					}
					else
					{
						Debug.Log("Maximum Amount data could not be parsed.");
					}

					break;
				}
				case LOOT_CHANCE_VAR_NAME:
				{
					bool foundVal = float.TryParse(module.data, out float val);
					if (foundVal)
					{
						lootChance = val;
					}
					else
					{
						Debug.Log("Maximum Amount data could not be parsed.");
					}

					break;
				}
			}

			return true;
		}

		public bool CheckSubtag(string filename, SaveTag subtag)
		{
			return false;
		}
	}
}