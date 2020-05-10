using InventorySystem;

namespace QuestSystem
{
	public class QuestDelivery : IDelivery
	{
		private DeliveryOrderDetails DeliveryDetails { get; set; }

		public QuestDelivery(IDelivery delivery)
		{
			DeliveryDetails = new DeliveryOrderDetails(
				delivery.GetNumberOfItems(),
				delivery.GetNumberOfUniqueItems(),
				delivery.GetOrderList());
		}

		public int GetNumberOfItems() => DeliveryDetails.GetNumberOfItems();

		public int GetNumberOfOrderEntries() => DeliveryDetails.GetNumberOfOrderEntries();

		public int GetNumberOfUniqueItems() => DeliveryDetails.GetNumberOfUniqueItems();

		public string GetOrderDetails() => DeliveryDetails.GetOrderDetails();

		public string[] GetOrderList() => DeliveryDetails.GetOrderList();
	}
}
