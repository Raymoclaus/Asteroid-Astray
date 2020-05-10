using System.Collections.Generic;

namespace InventorySystem
{
	public struct ItemCollection : IDelivery
	{
		/// <summary>
		/// Note: Once the constructor has run, the delivery details will not generate again.
		/// There is no reason to modify this collection
		/// </summary>
		public List<ItemStack> Stacks { get; private set; }
		private DeliveryOrderDetails DeliveryDetails { get; set; }

		public ItemCollection(List<ItemStack> stacks)
		{
			ItemStack.RemoveBlanks(stacks);
			Stacks = stacks;
			DeliveryDetails = new DeliveryOrderDetails(
				ItemStack.Count(stacks),
				ItemStack.GetNumberOfUniqueItems(stacks),
				ReadOrderList(stacks));

			string[] ReadOrderList(List<ItemStack> delivery)
			{
				string[] order = new string[delivery.Count];
				for (int i = 0; i < order.Length; i++)
				{
					order[i] = delivery[i].ToString();
				}
				return order;
			}
		}

		public ItemCollection(ItemStack stack) : this(new List<ItemStack>() {stack})
		{

		}

		public ItemCollection(ItemObject itemType) : this(new ItemStack(itemType))
		{

		}

		public int GetNumberOfItems() => DeliveryDetails.GetNumberOfItems();

		public int GetNumberOfUniqueItems() => DeliveryDetails.GetNumberOfUniqueItems();

		public int GetNumberOfOrderEntries() => DeliveryDetails.GetNumberOfOrderEntries();

		public string GetOrderDetails() => DeliveryDetails.GetOrderDetails();

		public string[] GetOrderList() => DeliveryDetails.GetOrderList();
	} 
}