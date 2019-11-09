using System;
using UnityEngine;

namespace AttackData
{
	public interface IAttacker
	{
		void ReceiveRecoil(Vector3 recoilVector);
		void ReceiveRecoveryDuration(float recoveryDuration);
		bool ShouldAttack(string action);
		float DamageMultiplier { get; }
	} 
}
