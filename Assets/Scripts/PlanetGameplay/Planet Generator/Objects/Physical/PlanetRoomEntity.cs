using System.Collections;
using System.Collections.Generic;
using AttackData;
using UnityEngine;

[RequireComponent(typeof(IPhysicsController))]
public abstract class PlanetRoomEntity : PlanetRoomObject
{
	private IPhysicsController physicsController;
	protected IPhysicsController PhysicsController
		=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());
	[SerializeField] protected Transform pivot;
	private Vector3 currentPosition;
	protected PlanetGenerator planetGenerator;
	private bool coolingDownFromAttack = false;

	protected virtual void Awake()
	{
		planetGenerator = FindObjectOfType<PlanetGenerator>();
		if (planetGenerator != null)
		{
			planetGenerator.OnGenerationComplete += GetActiveRoom;
		}
	}

	protected virtual void Update()
	{
		if (pivot.position != currentPosition)
		{
			UpdateRoomObjectPosition(pivot.position);
		}

		if (ShouldAttack())
		{
			Attack();
		}
	}

	protected IEnumerator SetAttackCooldown(WaitForSeconds wait)
	{
		coolingDownFromAttack = true;
		yield return wait;
		coolingDownFromAttack = false;
	}

	protected virtual bool ShouldAttack() => !coolingDownFromAttack;

	protected virtual void Attack() { }

	private void UpdateRoomObjectPosition(Vector2 position)
	{
		currentPosition = pivot.position;
		Vector2Int roundedPosition = new Vector2Int(Mathf.FloorToInt(position.x),
			Mathf.FloorToInt(position.y));
		SetRoomObjectWorldSpacePosition(roundedPosition);
	}

	public Vector3 GetPivotPosition() => pivot.position;

	private void GetActiveRoom() => room = planetGenerator.GetActiveRoom();

	private void OnTriggerEnter2D(Collider2D collision)
	{
		int layer = collision.gameObject.layer;
		AttackData.AttackManager atkM = collision.gameObject.GetComponent<AttackData.AttackManager>();
		if (layer == attackLayer)
		{
			TakeDamage(atkM);
		}
	}

	private void TakeDamage(AttackManager atkM)
	{
		if (atkM == null) return;
		if ((MonoBehaviour)atkM.GetData<AttackOwnerData>() == this) return;

		object knockback = atkM.GetData<AttackKnockbackData>();
		if (knockback != null)
		{
			PhysicsController.KnockBack((Vector3)knockback);
		}

		object stunDuration = atkM.GetData<AttackStunData>();
		if (stunDuration != null)
		{
			PhysicsController.SlowDown();
			PhysicsController.PreventMovementInputForDuration(
				new WaitForSeconds((float)stunDuration));
		}

		object pierceCount = atkM.GetData<AttackPierceData>();
		if (pierceCount == null || (int)pierceCount == 1)
		{
			Destroy(atkM.gameObject);
		}

		atkM.Hit();
	}
}
