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

	[SerializeField] private AttackManager attackPrefab;
	[SerializeField] private float meleeRange = 1f;
	[SerializeField] private float meleeAttackRecoveryTime = 1f;
	[SerializeField] private float meleeAttackCooldownTime = 3f;
	private string attackCooldownTimerID;
	[SerializeField] private float meleeAttackDamage = 10f;
	[SerializeField] private float meleeAttackStunDuration = 0.2f;

	protected override void Awake()
	{
		base.Awake();

		RollingBehaviour.OnRoll += Roll;
		attackCooldownTimerID = gameObject.GetInstanceID() + "Attack Cooldown Timer";
		TimerTracker.AddTimer(attackCooldownTimerID, 0f, null, null);
	}

	protected override void Update()
	{
		base.Update();

		if (!PlayerInRoom) return;

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

	protected override void FindPlayer()
	{
		base.FindPlayer();
		ChasingBehaviour.SetTarget(player?.Pivot);
		FleeingBehaviour.SetTarget(player?.Pivot);
	}

	private bool AttackOnCooldown => TimerTracker.GetTimer(attackCooldownTimerID) > 0f;

	private bool ShouldAttack
		=> !IsStunned
		&& !IsRolling
		&& !RecoveringFromAction;

	private bool ShouldMeleeAttack
		=> ShouldAttack
		&& DistanceToPlayer <= meleeRange
		&& !AttackOnCooldown;

	private void MeleeAttack()
	{
		AttackManager atkM = Instantiate(attackPrefab);

		Transform tr = atkM.transform;
		Vector3 movementDirection = PhysicsController.MovementDirection;
		Vector3 attackPos = GetPivotPosition() + movementDirection;
		tr.position = attackPos;
		float angle = Vector2.SignedAngle(Vector2.up, movementDirection);
		tr.eulerAngles = Vector3.forward * angle;

		atkM.AddAttackComponent<DamageComponent>(meleeAttackDamage);
		atkM.AddAttackComponent<DestroyAfterTimeComponent>(0.1f);
		atkM.AddAttackComponent<DirectionComponent>(movementDirection);
		atkM.AddAttackComponent<OwnerComponent>(this);
		atkM.AddAttackComponent<KnockbackComponent>(movementDirection * meleeAttackDamage);
		atkM.AddAttackComponent<StunComponent>(meleeAttackStunDuration);

		state = State.Fleeing;
		TimerTracker.SetTimer(actionRecoveryTimerID, meleeAttackRecoveryTime);
		TimerTracker.SetTimer(attackCooldownTimerID, meleeAttackCooldownTime);
		if (RecoveringFromAction)
		{
			PhysicsController.PreventMovementInputForDuration(meleeAttackRecoveryTime);
			PhysicsController.SlowDown();
		}
	}
}
