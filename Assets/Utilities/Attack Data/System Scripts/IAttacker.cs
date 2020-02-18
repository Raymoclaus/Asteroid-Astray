﻿using InputHandlerSystem;
using System;
using UnityEngine;

namespace AttackData
{
	public interface IAttacker
	{
		void ReceiveRecoil(Vector3 recoilVector);
		void ReceiveStoppingPower(float stoppingPower);
		void ReceiveRecoveryDuration(float recoveryDuration);
		bool ShouldAttack(InputAction action);
		float DamageMultiplier { get; }
	} 
}
