using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomTest : PlanetRoomEnemy
{
	private MovementBehaviour behaviour;
	private MovementBehaviour Behaviour
		=> behaviour ?? (behaviour = GetComponent<MovementBehaviour>());

	[SerializeField] private float rollIFrameDuration = 1f;
	[SerializeField] private float rollSpeed = 5f;

	protected override void Awake()
	{
		base.Awake();
		
		Behaviour.OnRoll += Roll;
	}

	protected override void Roll(Vector3 direction)
	{
		base.Roll(direction);

		PhysicsController.DeactivateColliderForDuration(rollIFrameDuration);
		PhysicsController.SetVelocity(direction * rollSpeed);
	}
}
