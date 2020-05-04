using InventorySystem;

namespace QuestSystem
{
	public class QuestDelivery : IDelivery
	{
		private string DeliveryDetails { get; set; }
		private int NumberOfOrderEntries { get; set; }
		private int NumberOfItems { get; set; }
		private int NumberOfUniqueItems { get; set; }
		private string[] OrderList { get; set; }

		public QuestDelivery(string deliveryDetails)
		{
			DeliveryDetails = deliveryDetails;

			string[] lines = DeliveryDetails.Split('\n');

			if (!int.TryParse(lines[0], out int numberOfOrderEntries)) return;
			NumberOfOrderEntries = numberOfOrderEntries;

			if (!int.TryParse(lines[1], out int numberOfItems)) return;
			NumberOfItems = numberOfItems;

			if (!int.TryParse(lines[2], out int numberOfUniqueItems)) return;
			NumberOfUniqueItems = numberOfUniqueItems;

			OrderList = new string[NumberOfOrderEntries];
			int orderListStartIndex = 3;
			for (int i = orderListStartIndex; i < lines.Length; i++)
			{
				int entryIndex = i - orderListStartIndex;
				OrderList[entryIndex] = lines[i];
			}
		}

		public QuestDelivery(IDelivery delivery) : this(delivery.GetOrderDetails())
		{

		}

		public int GetNumberOfItems() => NumberOfItems;

		public int GetNumberOfOrderEntries() => NumberOfOrderEntries;

		public int GetNumberOfUniqueItems() => NumberOfUniqueItems;

		public string GetOrderDetails() => DeliveryDetails;

		public string[] GetOrderList() => OrderList;
	}
}
