using UnityEngine;
using UnityEngine.Events;

namespace MovementBehaviours
{
	public class RepositionBehaviour : MovementBehaviour
	{
		public Vector3 repositionLocation;
		public bool IsRepositioning { get; private set; }
		private bool slowDownBeforeReachingPosition;
		private float goalRadius;
		public UnityEvent OnReachedGoal;

		protected virtual void Update()
		{
			if (IsRepositioning)
			{
				if (IsWithinGoalRadius)
				{
					SlowDown();
					IsRepositioning = false;
					OnReachedGoal?.Invoke();
				}
				else
				{
					MoveTowardsPosition(repositionLocation);
				}
			}
		}

		public void Reposition(Vector3 goal, float goalRadius = 1f, bool slowDownBeforeReachingPosition = false)
		{
			IsRepositioning = true;
			repositionLocation = goal;
			this.slowDownBeforeReachingPosition = slowDownBeforeReachingPosition;
			this.goalRadius = goalRadius;
		}

		protected float DistanceToGoal
			=> Vector3.Distance(repositionLocation, SelfPosition);

		protected bool IsWithinGoalRadius => DistanceToGoal <= goalRadius;
	}
}