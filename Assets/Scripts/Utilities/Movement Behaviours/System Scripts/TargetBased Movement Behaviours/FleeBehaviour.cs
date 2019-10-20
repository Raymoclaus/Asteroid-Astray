namespace MovementBehaviours
{
	public class FleeBehaviour : TargetBasedBehaviour
	{
		public override void TriggerUpdate()
		{
			base.TriggerUpdate();

			if (TargetIsNearby && ShouldRunAway)
			{
				MoveAwayFromPosition(TargetPosition);
			}
			else
			{
				SlowDown();
			}
		}

		protected virtual bool ShouldRunAway => true;
	}

}