using UnityEngine;

namespace AttackData
{
	public class DestroyOnDistanceFromTransformComponent : AttackComponent
	{
		private ComponentData data;

		public override void AssignData(object data = null)
		{
			base.AssignData(data);
			this.data = (ComponentData)data;
		}

		public override object GetData() => data;

		public override bool ShouldDestroy()
			=> Vector3.Distance(transform.position, data.transform.position)
			>= data.distanceLimit;

		public struct ComponentData
		{
			public Transform transform;
			public float distanceLimit;

			public ComponentData(Transform transform, float distanceLimit)
			{
				this.transform = transform;
				this.distanceLimit = distanceLimit;
			}
		}
	}
}
