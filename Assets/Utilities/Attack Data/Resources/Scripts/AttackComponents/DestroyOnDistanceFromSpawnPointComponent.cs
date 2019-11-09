using UnityEngine;

namespace AttackData
{
	public class DestroyOnDistanceFromSpawnPointComponent : AttackComponent
	{
		private Vector3 spawnPoint;
		private float distanceLimit = 10f;

		public override void AssignData(object data = null)
		{
			if (data == null) return;
			base.AssignData(data);
			distanceLimit = (float)data;
		}

		public override object GetData() => distanceLimit;

		public override bool ShouldDestroy()
			=> Vector3.Distance(spawnPoint, transform.position) >= distanceLimit;
	}
}
