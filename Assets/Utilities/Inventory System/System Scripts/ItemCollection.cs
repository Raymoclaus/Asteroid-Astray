using System.Collections.Generic;
using System.Text;

namespace InventorySystem
{
	public struct ItemCollection : IDelivery
	{
		private List<ItemStack> Collection { get; set; }
		private string DeliveryDetails { get; set; }
		private int NumberOfOrderEntries { get; set; }
		private int NumberOfItems { get; set; }
		private int NumberOfUniqueItems { get; set; }
		private string[] OrderList { get; set; }

		public ItemCollection(List<ItemStack> collection)
		{
			ItemStack.RemoveBlanks(collection);
			Collection = collection;
			NumberOfOrderEntries = collection.Count;
			NumberOfItems = ItemStack.Count(collection);
			NumberOfUniqueItems = ItemStack.GetNumberOfUniqueItems(collection);
			OrderList = ReadOrderList(collection);
			DeliveryDetails = CreateOrderDetails(NumberOfOrderEntries, NumberOfItems, NumberOfUniqueItems, OrderList);

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

		public ItemCollection(ItemStack stack)
		{
			Collection = new List<ItemStack>() { stack };
			NumberOfOrderEntries = 1;
			NumberOfItems = stack.Amount;
			NumberOfUniqueItems = 1;
			OrderList = new string[] { stack.ToString() };
			DeliveryDetails = CreateOrderDetails(NumberOfOrderEntries, NumberOfItems, NumberOfUniqueItems, OrderList); 
		}

		public List<ItemStack> GetCollectionCopy() => new List<ItemStack>(Collection);

		public string[] GetOrderList() => OrderList;

		public int GetNumberOfItems() => NumberOfItems;

		public int GetNumberOfUniqueItems() => NumberOfUniqueItems;

		public int GetNumberOfOrderEntries() => NumberOfOrderEntries;

		public string GetOrderDetails() => DeliveryDetails;

		private static string CreateOrderDetails(int numberOfOrderEntries, int numberOfItems, int numberOfUniqueItems, string[] orderList)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(numberOfOrderEntries);
			builder.AppendLine(numberOfItems.ToString());
			builder.AppendLine(numberOfUniqueItems.ToString());
			foreach (string line in orderList)
			{
				builder.AppendLine(line);
			}
			return builder.ToString();
		}
	} 
}