using TriggerSystem;

namespace InventorySystem
{
	public interface IItemHolder
	{
		ItemObject ItemType { get; set; }
		void SendItem(IInteractor interactor);
		void SendItem(IInventoryHolder inventoryHolder);
	}
}