using InputHandlerSystem;
using System;

namespace TriggerSystem
{
	public interface IActionTrigger : ITrigger
	{
		bool CanBeInteractedWith { get; }
		GameAction InteractAction { get; set; }
		void Interact(IInteractor interactor);
		event Action<IInteractor> OnInteracted;
	}

}