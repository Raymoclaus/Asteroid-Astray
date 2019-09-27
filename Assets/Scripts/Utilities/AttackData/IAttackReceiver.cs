using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
	public interface IAttackReceiver
	{
		void ReceiveAttack(AttackManager atkM);
		string LayerName { get; }
	}
}
