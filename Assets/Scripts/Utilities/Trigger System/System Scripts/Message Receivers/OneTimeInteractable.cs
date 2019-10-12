using UnityEngine;

namespace TriggerSystem.MessageReceivers
{
	public class OneTimeInteractable : InteractableObject
	{
		[SerializeField] private InteractionTrigger trigger;
		[SerializeField] private bool interactionCompleted;

		private void Awake()
		{
			trigger.TriggerEnabled = interactionCompleted;
		}

		protected override void PerformAction(IInteractor interactor)
		{
			interactionCompleted = true;
			trigger.TriggerEnabled = false;
		}
	}

}