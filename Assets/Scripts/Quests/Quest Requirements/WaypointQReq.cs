using UnityEngine;

public class WaypointQReq : QuestRequirement
{
	private string description;
	private Transform targetToReach;
	private Vector3? location;

	public WaypointQReq(Transform targetToReach, string description)
	{
		this.description = description;
		this.targetToReach = targetToReach;
	}

	public WaypointQReq(Vector3 location, string description)
	{
		this.description = description;
		this.location = location;
	}

	public override void Activate()
	{
		base.Activate();
		GameEvents.OnWaypointReached += EvaluateEvent;
	}

	private void EvaluateEvent(Vector3 location)
	{
		if (IsComplete() || !active) return;

		if (Vector3.Distance(location, (Vector3)TargetLocation()) < 5f)
		{
			QuestRequirementCompleted();
			GameEvents.OnWaypointReached -= EvaluateEvent;
		}
	}

	public override string GetDescription()
	{
		return description;
	}

	public override Vector3? TargetLocation()
	{
		return targetToReach != null ? targetToReach.position : location;
	}
}
