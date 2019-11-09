using System.Collections;
using System.Collections.Generic;
using AttackData;
using TriggerSystem;
using TriggerSystem.Triggers;
using UnityEngine;

namespace AttackData
{
	using IReceiver = IAttackMessageReceiver;

	public class AttackTrigger : VicinityTrigger, IAttackTrigger
	{
		private HashSet<IReceiver> receivers = new HashSet<IReceiver>();

		public string LayerName => LayerMask.LayerToName(gameObject.layer);

		protected override void EnteredTrigger(IActor actor)
		{
			base.EnteredTrigger(actor);

			if (actor is IAttackActor attack)
			{
				bool hit = false;
				foreach (IReceiver receiver in receivers)
				{
					if (receiver.ReceiveAttack(attack.GetAttackManager))
					{
						hit = true;
					}
				}

				if (hit)
				{
					attack.Hit(this);
				}
			}
		}

		protected override void GetReceivers()
		{
			base.GetReceivers();

			Transform t = transform;
			while (t != null)
			{
				foreach (IReceiver receiver in t.GetComponents<IReceiver>())
				{
					if (receiver.CanReceiveAttackMessagesFromLayer(TriggerLayer))
					{
						receivers.Add(receiver);
					}
				}
				t = t.parent;
			}
		}
	}
}