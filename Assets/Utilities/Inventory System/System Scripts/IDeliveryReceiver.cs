using System;

namespace InventorySystem
{
	public interface IDeliveryReceiver : IUnique
	{
		event Action<IDeliverer, IDelivery> OnDeliveryReceived;
		bool IsExpectingDelivery(IDeliverer deliverer, IDelivery delivery);
		void ExpectDelivery(IDeliverer deliverer, IDelivery delivery);
		bool ReceiveDelivery(IDeliverer deliverer, IDelivery delivery);
	} 
}
