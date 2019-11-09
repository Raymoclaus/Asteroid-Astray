namespace TriggerSystem
{
	public interface ITriggerMessageReceiver
	{
		void EnteredTrigger(IActor actor);
		void ExitedTrigger(IActor actor);
		void AllExitedTrigger();
		bool CanReceiveMessagesFromLayer(int layer);
	}
}