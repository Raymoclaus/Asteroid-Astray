using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class VelocityComponent : AttackComponent
	{
		private Vector3 velocity;
		private Rigidbody2D rb;
		private Rigidbody2D Rb => rb ?? (rb = GetComponent<Rigidbody2D>());

		public override void AssignData(object data) => velocity = (Vector3)data;

		public override object GetData() => velocity;

		private void Update()
		{
			if (Rb != null)
			{
				Rb.velocity = velocity;
			}
			else
			{
				Debug.Log("No rigidbody found");
			}
		}

		public override bool ShouldDestroy() => false;
	}
}
