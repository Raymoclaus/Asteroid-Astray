using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace InventorySystem
{
	public static class Item
	{
		public const int MAX_RARITY = 10;

		public const int MIN_RARITY = 0;

		private static ItemObject[] itemTypes;
		public static ItemObject[] ItemTypes
			=> itemTypes != null
				? itemTypes
				: (itemTypes = Resources.LoadAll<ItemObject>(string.Empty));

		public static ItemObject GetItemByName(string name, bool caseSensitive = false)
		{
			if (caseSensitive)
			{
				return ItemTypes.FirstOrDefault(t => t.ItemName == name);
			}
			return ItemTypes.FirstOrDefault(t => t.ItemName.ToLower() == name.ToLower());
		}

		public static string TypeName(ItemObject item) => item.ItemName;

		public static int TypeRarity(ItemObject item) => item.Rarity;

		public static int StackLimit(ItemObject item) => item.StackLimit;

		public static bool IsKeyItem(ItemObject item) => item.IsKeyItem;

		public static string Description(ItemObject item) => item.Description;

		public static string FlavourText(ItemObject item) => item.FlavourText;

		public static Sprite GetItemSprite(ItemObject item) => item.Icon;
	}
}