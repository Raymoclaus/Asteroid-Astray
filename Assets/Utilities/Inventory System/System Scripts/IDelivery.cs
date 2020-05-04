namespace InventorySystem
{
	public interface IDelivery
	{
		string[] GetOrderList();
		int GetNumberOfItems();
		int GetNumberOfUniqueItems();
		int GetNumberOfOrderEntries();
		string GetOrderDetails();
	} 
}
