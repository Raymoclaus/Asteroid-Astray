using UnityEngine;

namespace InventorySystem
{
	public struct DeliveryOrderDetails : IDelivery
	{
		private int _numberOfOrderEntries, _numberOfItems, _numberOfUniqueItems;
		private string[] _orderList;
		private string _orderDetails;

		public DeliveryOrderDetails(int numberOfItems, int numberOfUniqueItems,
			string[] orderList)
		{
			_orderList = orderList;
			_numberOfOrderEntries = orderList.Length;
			_numberOfItems = numberOfItems;
			_numberOfUniqueItems = numberOfUniqueItems;
			_orderDetails = DeliveryOrderDetailsGenerator.Generate(_numberOfItems, _numberOfUniqueItems, _orderList);
		}

		public DeliveryOrderDetails(string deliveryDetails)
		{
			_orderDetails = deliveryDetails;
			bool successful = DeliveryOrderDetailsGenerator.TryParse(
				deliveryDetails,
				out _numberOfItems,
				out _numberOfUniqueItems,
				out _orderList);

			if (successful)
			{
				_numberOfOrderEntries = _orderList.Length;
			}
			else
			{
				Debug.LogWarning($"Order details parsed unsuccessfully.\n{deliveryDetails}");
				_numberOfOrderEntries = 0;
			}
		}

		public int GetNumberOfOrderEntries() => _numberOfOrderEntries;

		public int GetNumberOfItems() => _numberOfItems;

		public int GetNumberOfUniqueItems() => _numberOfUniqueItems;

		public string[] GetOrderList() => _orderList;

		public string GetOrderDetails() => _orderDetails;
	} 
}
