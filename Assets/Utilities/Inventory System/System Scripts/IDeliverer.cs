namespace InventorySystem
{
	public interface IDeliverer : IUnique
	{
		bool Deliver(IDelivery delivery, IDeliveryReceiver receiver);
	}
}