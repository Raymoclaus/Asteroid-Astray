using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackData;

[RequireComponent(typeof(ChasingBehaviour))]
public class PlanetRoomChasingEnemy : PlanetRoomEnemy
{
	private ChasingBehaviour movementBehaviour;
	protected ChasingBehaviour MovementBehaviour
		=> movementBehaviour ?? (movementBehaviour = GetComponent<ChasingBehaviour>());

	protected override bool ShouldAttack()
	{
		return base.ShouldAttack()
			&& DistanceToPlayer <= attackRange
			&& player.GetRoom() == GetRoom();
	}

	protected override void Attack()
	{
		base.Attack();

		ProjectileAttack attackObj = Instantiate(attackPrefab);

		Vector3 direction = PhysicsController.GetDirection();
		Vector3 facingDirection = PhysicsController.GetFacingDirection();
		attackObj.SetVelocity(direction * 6f);
		attackObj.transform.position =
			pivot.position + facingDirection;
		attackObj.transform.parent = transform;
		attackObj.transform.eulerAngles =
			Vector3.forward * Vector2.SignedAngle(Vector2.up, facingDirection);

		AttackManager atkM = attackObj.GetComponent<AttackManager>();
		float damage = 20f;
		atkM.AddAttackComponent<AttackDamageData>(damage);
		atkM.AddAttackComponent<AttackKnockbackData>(direction * damage);
		atkM.AddAttackComponent<AttackStunData>(0.8f);
		atkM.AddAttackComponent<AttackOwnerData>(this);
		atkM.AddAttackComponent<AttackPierceData>(1);

		PhysicsController.PreventMovementInputForDuration(new WaitForSeconds(0.5f));
		PhysicsController.SlowDown();

		StartCoroutine(SetAttackCooldown(new WaitForSeconds(2f)));
	}
}
