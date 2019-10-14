using TriggerSystem;

namespace InventorySystem
{
	public interface IItemHolder
	{
		Item.Type ItemType { get; set; }
		void SendItem(IInteractor interactor);
		void SendItem(IInventoryHolder inventoryHolder);
	}
}