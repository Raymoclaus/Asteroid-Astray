using System;
using UnityEngine;

namespace QuestSystem
{
	public interface IWaypoint : IUnique
	{
		event Action OnWaypointReached;
		string ExpectedActorID { get; }
		Vector3 Position { get; }
	}
}
