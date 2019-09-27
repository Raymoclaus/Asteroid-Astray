using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionBehaviour : MovementBehaviour
{
	private Vector3 repositionLocation;
	public bool IsRepositioning { get; private set; }
	private bool slowDownBeforeReachingPosition;
	private float goalRadius;

	protected virtual void Update()
	{
		if (IsRepositioning)
		{
			if (IsWithinGoalRadius)
			{
				SlowDown();
				IsRepositioning = false;
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
