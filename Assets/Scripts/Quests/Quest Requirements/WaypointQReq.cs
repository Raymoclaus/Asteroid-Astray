using UnityEngine;

public class WaypointQReq : QuestRequirement
{
	private string description;
	private Waypoint waypoint;

	public WaypointQReq(Waypoint waypoint, string description)
	{
		this.description = description;
		this.waypoint = waypoint;
	}

	public override void Activate()
	{
		base.Activate();
		GameEvents.OnWaypointReached += EvaluateEvent;
	}

	private void EvaluateEvent(Waypoint waypoint)
	{
		if (Completed || !active) return;

		if (this.waypoint == waypoint)
		{
			QuestRequirementCompleted();
			GameEvents.OnWaypointReached -= EvaluateEvent;
		}
	}

	public override string GetDescription() => description;

	public override Vector3? TargetLocation() => waypoint.GetPosition();
}
