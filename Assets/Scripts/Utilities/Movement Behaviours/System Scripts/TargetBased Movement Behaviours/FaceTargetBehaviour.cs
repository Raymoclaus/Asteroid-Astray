namespace MovementBehaviours
{
	public class FaceTargetBehaviour : TargetBasedBehaviour
	{
		private void Update()
		{
			FaceDirection(TargetDirection);
		}
	}

}