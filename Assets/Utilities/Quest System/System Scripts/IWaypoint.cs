using SaveSystem;
using System;
using UnityEngine;

namespace QuestSystem
{
	public interface IWaypoint : IUnique
	{
		event Action OnWaypointReached;
		string ExpectedActorID { get; }
		string PrefabType { get; set; }
		Vector3 Position { get; }
		void Remove();
		void Save(string filename, SaveTag parentTag);
		bool ApplyData(DataModule module);
		bool CheckSubtag(string filename, SaveTag subtag);
	}
}
