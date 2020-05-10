using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public static class Item
	{
		public const int MAX_RARITY = 10;

		public const int MIN_RARITY = 0;

		private static Dictionary<string, ItemObject> _itemTypes;
		public static Dictionary<string, ItemObject> ItemTypes
		{
			get
			{
				if (_itemTypes != null) return _itemTypes;
				ItemObject[] items = Resources.LoadAll<ItemObject>(string.Empty);
				_itemTypes = new Dictionary<string, ItemObject>();
				foreach (ItemObject item in items)
				{
					_itemTypes.Add(item.ItemName, item);
				}

				return _itemTypes;
			}
		}

		public static ItemObject GetItemByName(string name)
		{
			if (!ItemTypes.ContainsKey(name)) return ItemObject.Blank;
			return ItemTypes[name];
		}

		public static string GetTypeName(this ItemObject item) => item?.ItemName ?? default;

		public static int GetTypeRarity(this ItemObject item) => item?.Rarity ?? default;

		public static int GetStackLimit(this ItemObject item) => item?.StackLimit ?? default;

		public static bool GetIsKeyItem(this ItemObject item) => item?.IsKeyItem ?? default;

		public static string GetDescription(this ItemObject item) => item?.Description ?? default;

		public static string GetFlavourText(this ItemObject item) => item?.FlavourText ?? default;

		public static Sprite GetItemSprite(this ItemObject item) => item?.Icon;
	}
}