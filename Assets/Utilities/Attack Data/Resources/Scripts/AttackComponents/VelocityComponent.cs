using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class VelocityComponent : AttackComponent
	{
		public Vector3 velocity;
		private Rigidbody2D rb;
		public Rigidbody2D Rb => rb ?? (rb = GetComponent<Rigidbody2D>());

		public override void AssignData(object data) => velocity = (Vector3)data;

		public override object GetData() => velocity;

		private void Update()
		{
			UpdateVelocity();
		}

		private void UpdateVelocity()
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

		private void FixedUpdate()
		{
			UpdateVelocity();
		}

		public override bool ShouldDestroy() => false;
	}
}
