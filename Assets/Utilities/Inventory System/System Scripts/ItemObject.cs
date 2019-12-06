using UnityEngine;

namespace InventorySystem
{
	[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Inventory System/Item")]
	public class ItemObject : ScriptableObject
	{
		public const ItemObject Blank = null;

		[SerializeField] private Sprite icon;
		[SerializeField] private int rarity = 1;
		[SerializeField] private int stackLimit = 100;
		[SerializeField] private bool isKeyItem;
		[SerializeField] [TextArea] private string description = "<No description provided>";
		[SerializeField] [TextArea] private string flavourText = "<No flavour text provided>";

		public Sprite Icon => icon;
		public string ItemName => name;
		public int Rarity => rarity;
		public int StackLimit => stackLimit;
		public bool IsKeyItem => isKeyItem;
		public string Description => description;
		public string FlavourText => flavourText;
	} 
}
