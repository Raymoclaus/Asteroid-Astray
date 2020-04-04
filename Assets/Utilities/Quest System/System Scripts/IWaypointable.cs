using UnityEngine;

namespace QuestSystem
{
	public interface IWaypointable : IUnique
	{
		Vector3 Position { get; }
	} 
}
