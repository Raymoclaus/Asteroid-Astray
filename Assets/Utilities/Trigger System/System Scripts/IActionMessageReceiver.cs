namespace TriggerSystem
{
	public interface IActionMessageReceiver
	{
		void Interacted(IInteractor interactor, string action);
	}

}