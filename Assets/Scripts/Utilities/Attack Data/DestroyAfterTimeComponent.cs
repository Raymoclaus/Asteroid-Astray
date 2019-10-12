using UnityEngine;

namespace AttackData
{
	public class DestroyAfterTimeComponent : AttackComponent
	{
		private float timer = 1f;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			timer = (float)data;
		}

		public override object GetData() => timer;

		private void Update()
		{
			timer -= Time.deltaTime;
		}

		public override bool ShouldDestroy() => timer <= 0f;
	}
}
