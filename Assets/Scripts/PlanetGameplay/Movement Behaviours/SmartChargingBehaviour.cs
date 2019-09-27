using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartChargingBehaviour : ChargingBehaviour
{
	private Vector3 targetLocationWhenStartedCharging;
	private float previousDistanceToTargetChargingLocation;
	private float previousDistanceToTarget;

	protected override void Update()
	{
		base.Update();

		if (IsCharging)
		{
			float currentDistanceToTarget = DistanceToTargetSquared;
			float currentDistanceToTargetChargingLocation =
				Vector3.SqrMagnitude(SelfPosition - targetLocationWhenStartedCharging);
			if (currentDistanceToTargetChargingLocation
				> previousDistanceToTargetChargingLocation
				&& currentDistanceToTarget > previousDistanceToTarget)
			{
				StopCharging(false);
			}
			previousDistanceToTargetChargingLocation = currentDistanceToTargetChargingLocation;
			previousDistanceToTarget = currentDistanceToTarget;
		}
	}

	protected override void StartCharging()
	{
		base.StartCharging();

		targetLocationWhenStartedCharging = TargetPosition;
		previousDistanceToTargetChargingLocation = DistanceToTargetSquared;
		previousDistanceToTarget = previousDistanceToTargetChargingLocation;
	}
}
