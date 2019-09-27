using System;
using System.Collections.Generic;
using AttackData;
using UnityEngine;

[RequireComponent(typeof(IPhysicsController))]
public abstract class PlanetRoomEntity : PlanetRoomObject, IAttackReceiver
{
	private IPhysicsController physicsController;
	protected IPhysicsController PhysicsController
		=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());
	[SerializeField] protected Transform pivot;
	private Vector3 currentPosition;
	protected PlanetGenerator planetGenerator;
	[SerializeField] private Collider2D hitbox;
	private string iFrameTimerID;

	private static int entityLayer = -1;
	protected static int EntityLayer => entityLayer == -1 ?
		entityLayer = LayerMask.NameToLayer("Entity")
		: entityLayer;

	protected bool IsStunned { get; set; }
	private string stunTimerID;

	protected virtual void Awake()
	{
		stunTimerID = gameObject.GetInstanceID() + "StunTimer";
		TimerTracker.AddTimer(stunTimerID, 0f, () => IsStunned = false, null);
		iFrameTimerID = gameObject.GetInstanceID() + "IFrame Timer";
		TimerTracker.AddTimer(iFrameTimerID, 0f, () => EnableHitbox = true, null);

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
	}

	private void UpdateRoomObjectPosition(Vector2 position)
	{
		currentPosition = pivot.position;
		Vector2Int roundedPosition = new Vector2Int(Mathf.FloorToInt(position.x),
			Mathf.FloorToInt(position.y));
		SetRoomObjectWorldSpacePosition(roundedPosition);
	}

	protected virtual void DestroySelf() => room.RemoveObject(roomObject);

	public Transform Pivot => pivot ?? transform;

	public Vector3 GetPivotPosition() => Pivot.position;

	private void GetActiveRoom() => room = planetGenerator.GetActiveRoom();

	protected bool EnableHitbox
	{
		get { return hitbox.enabled; }
		set { if (hitbox != null) hitbox.enabled = value; }
	}

	protected void DeactivateHitboxForDuration(float duration)
	{
		EnableHitbox = false;
		TimerTracker.SetTimer(iFrameTimerID, duration);
	}

	protected virtual void TakeHit(AttackManager atkM)
	{
		if (atkM == null) return;
		if (!CanTakeHit) return;
		if (!VerifyAttack(atkM)) return;

		TakeKnockbackHit(atkM.GetData<KnockbackComponent>());
		TakeStunHit(atkM.GetData<StunComponent>());

		atkM.Hit(this);
	}

	protected virtual bool CanTakeHit => EnableHitbox;

	protected virtual bool VerifyAttack(AttackManager atkM)
		=> (MonoBehaviour)atkM.GetData<OwnerComponent>() != this;

	protected virtual void TakeKnockbackHit(object knockbackDataObj)
	{
		if (knockbackDataObj == null) return;
		Vector3 knockback = (Vector3)knockbackDataObj;
		PhysicsController.SetVelocity(knockback);
	}

	protected virtual void TakeStunHit(object stunDataObj)
	{
		if (stunDataObj == null) return;
		float stunDuration = (float)stunDataObj;
		IsStunned = true;
		float currentStunTime = TimerTracker.GetTimer(stunTimerID);
		if (currentStunTime >= stunDuration) return;
		TimerTracker.SetTimer(stunTimerID, stunDuration);
		PhysicsController.SlowDown();
		PhysicsController.PreventMovementInputForDuration(stunDuration);
	}

	protected virtual void Roll(Vector3 direction)
	{
		if (IsStunned) return;
	}

	public void ReceiveAttack(AttackManager atkM) => TakeHit(atkM);

	public string LayerName => LayerMask.LayerToName(gameObject.layer);
}
