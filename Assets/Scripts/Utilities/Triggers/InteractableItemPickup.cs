using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemPickup))]
public class InteractableItemPickup : DestructibleInteractableObject
{
	protected override void PerformAction(IInteractor interactor)
	{
		GetComponent<ItemPickup>().SendItem(interactor);
		base.PerformAction(interactor);
	}
}
