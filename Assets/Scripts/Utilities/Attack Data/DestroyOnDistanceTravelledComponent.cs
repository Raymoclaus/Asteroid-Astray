using UnityEngine;

namespace AttackData
{
	public class DestroyOnDistanceTravelledComponent : AttackComponent
	{
		private float distanceLimit = 10f;
		private float distanceTravelled;
		private Vector3 previousPosition;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			distanceLimit = (float)data;
			previousPosition = transform.position;
		}

		public override object GetData() => distanceTravelled;

		public override bool ShouldDestroy() => distanceTravelled >= distanceLimit;

		private void Update()
		{
			Vector3 currentPosition = transform.position;
			distanceTravelled += Vector3.Distance(previousPosition, currentPosition);
			previousPosition = currentPosition;
		}
	}
}
