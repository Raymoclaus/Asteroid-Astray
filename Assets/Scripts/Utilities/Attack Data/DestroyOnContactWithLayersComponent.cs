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

		public override void Hit(IAttackReceiver target)
		{
			base.Hit(target);
			if (data.ContainsLayer(target.LayerName))
			{
				contactedSpecifiedLayers = true;
			}
		}

		public override bool ShouldDestroy() => contactedSpecifiedLayers;

		public override void Contact(Collider2D target)
		{
			base.Contact(target);
			string layerName = LayerMask.LayerToName(target.gameObject.layer);
			if (data.ContainsLayer(layerName))
			{
				contactedSpecifiedLayers = true;
			}
		}

		public override bool VerifyTarget(IAttackReceiver target) => true;
	}
}
