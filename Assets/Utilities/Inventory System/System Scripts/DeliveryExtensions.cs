namespace InventorySystem
{
	public static class DeliveryExtensions
	{
		public static bool Matches(this IDelivery source, IDelivery other)
		{
			return source.GetOrderDetails() == other.GetOrderDetails();
		}
	}
}
