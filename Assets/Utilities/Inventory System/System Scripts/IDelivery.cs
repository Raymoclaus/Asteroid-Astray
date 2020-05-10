namespace InventorySystem
{
	public interface IDelivery
	{
		int GetNumberOfItems();
		int GetNumberOfUniqueItems();
		int GetNumberOfOrderEntries();
		string GetOrderDetails();
		string[] GetOrderList();
	} 
}
