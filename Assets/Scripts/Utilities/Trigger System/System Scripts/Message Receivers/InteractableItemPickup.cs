using UnityEngine;

namespace TriggerSystem.MessageReceivers
{
	[RequireComponent(typeof(ItemPickup))]
	public class InteractableItemPickup : DestructibleInteractableObject
	{
		protected override void PerformAction(IInteractor interactor)
		{
			GetComponent<ItemPickup>().SendItem(interactor);
			base.PerformAction(interactor);
		}
	}

}