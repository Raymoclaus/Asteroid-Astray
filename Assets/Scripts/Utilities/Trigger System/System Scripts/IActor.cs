namespace TriggerSystem
{
	public interface IActor
	{
		void EnteredTrigger(ITrigger vTrigger);
		void ExitedTrigger(ITrigger vTrigger);
		bool CanTriggerPrompts { get; }
	}

}