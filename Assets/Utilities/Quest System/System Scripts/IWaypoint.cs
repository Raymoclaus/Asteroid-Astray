using System;
using UnityEngine;

namespace QuestSystem
{
	public interface IWaypoint
	{
		event Action OnWaypointReached;
		Vector3 WaypointPosition { get; }
	}
}
