using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetBasedBehaviour : MovementBehaviour
{
	[SerializeField] protected Transform target;
	[SerializeField] protected float behaviourActivationRange = 10f;

	public void SetTarget(Transform target) => this.target = target;

	protected virtual bool TargetIsNearby
		=> target == null ? false : DistanceToTarget <= behaviourActivationRange;

	protected float DistanceToTarget
		=> (TargetPosition - SelfPosition).magnitude;

	protected float DistanceToTargetSquared
		=> (TargetPosition - SelfPosition).sqrMagnitude;

	protected Vector3 TargetPosition => target?.position ?? Vector3.zero;

	protected Vector3 TargetDirection => (TargetPosition - SelfPosition).normalized;
}
