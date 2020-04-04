using System;

namespace TriggerSystem
{
	public interface IInteractable : IUnique
	{
		event Action<IInteractor> OnInteracted;
	} 
}
