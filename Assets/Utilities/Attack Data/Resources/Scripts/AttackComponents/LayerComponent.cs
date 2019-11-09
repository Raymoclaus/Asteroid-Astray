using UnityEngine;

namespace AttackData
{
	public class LayerComponent : AttackComponent
	{
		public ComponentData data;

		public override void AssignData(object data = null)
		{
			base.AssignData(data);
			this.data = (ComponentData)data;
		}

		public override object GetData() => data;

		public override bool VerifyTarget(IAttackTrigger target)
			=> base.VerifyTarget(target) && data.ContainsLayer(target.LayerName);

		[System.Serializable]
		public struct ComponentData
		{
			public LayerMask layerMask;

			public ComponentData(params string[] layerNames)
			{
				layerMask = 0;
				for (int i = 0; i < layerNames.Length; i++)
				{
					layerMask = AddLayer(layerNames[i]);
				}
			}

			public ComponentData(params int[] layers)
			{
				layerMask = 0;
				for (int i = 0; i < layers.Length; i++)
				{
					layerMask = AddNonBitShiftedLayer(layers[i]);
				}
			}

			/// <summary>
			/// Adds given bit-shifted layer into the layer mask
			/// </summary>
			private int AddBitShiftedLayer(int bitShiftedLayer)
				=> layerMask | bitShiftedLayer;

			/// <summary>
			/// Adds given non-bit-shifted layer into the layer mask
			/// </summary>
			private int AddNonBitShiftedLayer(int nonBitShiftedLayer)
				=> AddBitShiftedLayer(1 << nonBitShiftedLayer);

			private int AddLayer(string layerName)
				=> AddNonBitShiftedLayer(LayerMask.NameToLayer(layerName));

			public bool ContainsLayer(string layerName)
				=> (layerMask & (1 << LayerMask.NameToLayer(layerName))) != 0;
		}
	}
}