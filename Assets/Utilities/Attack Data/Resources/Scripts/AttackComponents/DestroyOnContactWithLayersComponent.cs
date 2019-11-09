using UnityEngine;

namespace AttackData
{
	public class DestroyOnContactWithLayersComponent : LayerComponent
	{
		private ComponentData data;
		private bool contactedSpecifiedLayers;

		public override void AssignData(object data = null)
		{
			if (data == null) return;
			base.AssignData(data);
			this.data = (ComponentData)data;
		}

		public override object GetData() => data;

		public override void Hit(IAttackTrigger target)
		{
			base.Hit(target);
			if (data.ContainsLayer(target.LayerName))
			{
				contactedSpecifiedLayers = true;
			}
		}

		public override bool ShouldDestroy() => contactedSpecifiedLayers;

		public override bool VerifyTarget(IAttackTrigger target) => true;
	}
}
