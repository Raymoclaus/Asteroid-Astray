using System.Collections;
using System.Collections.Generic;
using AttackData;
using UnityEngine;

[RequireComponent(typeof(FollowBehaviour))]
[RequireComponent(typeof(RollTowardsProjectileBehaviour))]
[RequireComponent(typeof(CriticalDistanceBehaviour))]
public class PlanetRoomSpooder : PlanetRoomEnemy
{
	private enum State { Chasing, Fleeing }
	private State state = State.Chasing;

	private FollowBehaviour chasingBehaviour;
	private FollowBehaviour ChasingBehaviour
		=> chasingBehaviour ?? (chasingBehaviour = GetComponent<FollowBehaviour>());

	private RollTowardsProjectileBehaviour rollingBehaviour;
	private RollTowardsProjectileBehaviour RollingBehaviour
		=> rollingBehaviour ?? (rollingBehaviour = GetComponent<RollTowardsProjectileBehaviour>());

	private CriticalDistanceBehaviour fleeingBehaviour;
	private CriticalDistanceBehaviour FleeingBehaviour
		=> fleeingBehaviour ?? (fleeingBehaviour = GetComponent<CriticalDistanceBehaviour>());

	[SerializeField] private float rollIFrameDuration = 0.4f;
	[SerializeField] private float rollSpeed = 0.3f;
	[SerializeField] private float rollDuration = 1f;
	private string rollTimerID;
	private Vector3 RollDirection { get; set; }
	[SerializeField] private float meleeRange = 1f;
	[SerializeField] private float meleeAttackRecoveryTime = 1f;
	[SerializeField] private float meleeAttackCooldownTime = 3f;
	private string attackRecoveryTimerID, attackCooldownTimerID;

	protected override void Awake()
	{
		base.Awake();

		RollingBehaviour.OnRoll += Roll;
		attackRecoveryTimerID = gameObject.GetInstanceID() + "Attack Recovery Timer";
		TimerTracker.AddTimer(attackRecoveryTimerID, 0f, null, null);
		attackCooldownTimerID = gameObject.GetInstanceID() + "Attack Cooldown Timer";
		TimerTracker.AddTimer(attackCooldownTimerID, 0f, null, null);
		rollTimerID = gameObject.GetInstanceID() + "Roll Timer";
		TimerTracker.AddTimer(rollTimerID, 0f, null, null);

		ChasingBehaviour.SetTarget(player.Pivot);
		FleeingBehaviour.SetTarget(player.Pivot);
	}

	protected override void Update()
	{
		base.Update();

		if (player.Room != room) return;

		switch (state)
		{
			case State.Chasing:
				if (IsRolling) break;
				ChasingBehaviour.TriggerUpdate();
				if (ShouldMeleeAttack)
				{
					MeleeAttack();
				}
				break;
			case State.Fleeing:
				FleeingBehaviour.TriggerUpdate();
				if (!AttackOnCooldown)
				{
					state = State.Chasing;
				}
				break;
		}
	}

	protected void FixedUpdate()
	{
		if (IsRolling)
		{
			float delta = RollTimer / rollDuration;
			PhysicsController.SetVelocity(RollDirection * rollSpeed * delta);
		}
	}

	private bool AttackOnCooldown => TimerTracker.GetTimer(attackCooldownTimerID) > 0f;

	private bool RecoveringFromAttack => TimerTracker.GetTimer(attackRecoveryTimerID) > 0f;

	private bool ShouldAttack
		=> !IsStunned
		&& !IsRolling
		&& TimerTracker.GetTimer(attackRecoveryTimerID) <= 0f;

	private bool ShouldMeleeAttack
		=> ShouldAttack
		&& DistanceToPlayer <= meleeRange
		&& !AttackOnCooldown;

	private float RollTimer => TimerTracker.GetTimer(rollTimerID);

	private bool IsRolling => RollTimer > 0f;

	private void MeleeAttack()
	{
		Debug.Log("Attack");

		state = State.Fleeing;
		TimerTracker.SetTimer(attackRecoveryTimerID, meleeAttackRecoveryTime);
		TimerTracker.SetTimer(attackCooldownTimerID, meleeAttackCooldownTime);
		if (RecoveringFromAttack)
		{
			PhysicsController.PreventMovementInputForDuration(meleeAttackRecoveryTime);
			PhysicsController.SlowDown();
		}
	}

	protected override void Roll(Vector3 direction)
	{
		if (!CanRoll) return;
		base.Roll(direction);

		TimerTracker.SetTimer(rollTimerID, rollDuration);
		if (IsRolling)
		{
			RollDirection = direction;
			DeactivateHitboxForDuration(rollIFrameDuration);
		}
	}

	protected bool CanRoll
		=> !IsStunned
		&& !RecoveringFromAttack
		&& !IsRolling;
}
