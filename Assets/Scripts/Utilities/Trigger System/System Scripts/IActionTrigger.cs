using System;

namespace TriggerSystem
{
	public interface IActionTrigger : ITrigger
	{
		bool CanBeInteractedWith { get; }
		string ActionRequired { get; }
		void Interact(IInteractor interactor);
		event Action<IInteractor> OnInteracted;
	}

}