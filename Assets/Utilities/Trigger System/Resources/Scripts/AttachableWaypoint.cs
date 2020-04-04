using QuestSystem;

namespace TriggerSystem
{
	public class AttachableWaypoint : VicinityWaypoint
	{
		public IWaypointable AttachedWaypointable { get; set; }
		public bool RemoveOnDetach { get; set; } = true;

		private void Update()
		{
			if (AttachedWaypointable == null || AttachedWaypointable.Equals(null))
			{
				Detach(RemoveOnDetach);
				return;
			}

			Position = AttachedWaypointable.Position;
		}

		public void Detach(bool? remove = null)
		{
			if (remove != null)
			{
				RemoveOnDetach = (bool)remove;
			}

			AttachedWaypointable = null;

			if (RemoveOnDetach)
			{
				Remove();
			}
		}

		private void FindWaypointable(string ID)
		{
			IUnique obj = UniqueIDGenerator.GetObjectByID(ID);
			IWaypointable waypointable = obj as IWaypointable;
			if (waypointable == null) return;
			AttachedWaypointable = waypointable;
		}
	} 
}
