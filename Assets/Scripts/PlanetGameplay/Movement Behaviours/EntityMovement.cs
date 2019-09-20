using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterAnimationController))]
public class EntityMovement : MonoBehaviour, IPhysicsController
{
	public InputHandler.InputCode code;
	private Rigidbody2D rb;
	private Rigidbody2D Rb => rb ?? (rb = GetComponent<Rigidbody2D>());
	private CharacterAnimationController cac;
	private CharacterAnimationController Cac
		=> cac ?? (cac = GetComponent<CharacterAnimationController>());

	[SerializeField] private Transform selfPivot;
	private Transform SelfPivot => selfPivot != null ? selfPivot : transform;
	public Vector3 SelfPosition { get { return SelfPivot.position; } }

	[SerializeField] protected float speed = 1f;
	[SerializeField] private float momentumControl = 1f;
	[Range(0f, 1f)]
	[SerializeField] private float stoppingMomentumMultiplier = 0.5f;
	private bool moving = false;
	public bool CanMove { get; private set; } = true;
	private Vector2 direction;
	public Direction Facing
	{
		get
		{
			switch(directionID)
			{
				default: return Direction.Up;
				case 0: return Direction.Up;
				case 1: return Direction.Right;
				case 2: return Direction.Down;
				case 3: return Direction.Left;
			}
		}
	}
	public Vector2Int DirectionValue
	{
		get
		{
			switch (Facing)
			{
				default: return Vector2Int.up;
				case Direction.Up: return Vector2Int.up;
				case Direction.Right: return Vector2Int.right;
				case Direction.Down: return Vector2Int.down;
				case Direction.Left: return Vector2Int.left;
			}
		}
	}
	public Vector2 DirectionFloatValue => new Vector2(DirectionValue.x, DirectionValue.y);
	private int directionID;

	protected virtual void Update()
	{
		if (!moving)
		{
			AdjustVelocity(Vector2.zero);
		}
	}

	private void AdjustVelocity(Vector2 direction)
	{
		float distance = Vector2.Distance(Rb.velocity, direction);
		if (direction == Vector2.zero)
		{
			distance = 0f;
		}
		float delta = Mathf.Lerp(stoppingMomentumMultiplier, 1f, 1f - 1f / distance);
		SetVelocity(Vector2.MoveTowards(
			Rb.velocity, direction, momentumControl * delta / Rb.mass));
	}

	public void MoveAwayFromPosition(Vector3 targetPosition)
	{
		Vector2 direction = SelfPosition - targetPosition;
		MoveInDirection(direction);
	}

	public void MoveInDirection(Vector3 direction)
		=> MoveTowardsPosition(SelfPosition + direction, false);

	public void MoveTowardsPosition(Vector3 targetPosition, bool slowDownBeforeReachingPosition)
	{
		Vector2 direction = targetPosition - SelfPosition;
		float distanceToPosition = direction.magnitude;
		direction.Normalize();
		float adjustedSpeed = slowDownBeforeReachingPosition ?
			Mathf.Min(speed, distanceToPosition) : speed;
		Move(direction * adjustedSpeed);
	}

	private void Move(Vector2 direction)
	{
		if (!CanMove) return;

		moving = true;
		AdjustVelocity(direction);
		FaceDirection(direction);
		Cac?.SetRunning(true);
	}

	private int ConvertDirectionToInt(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x);
		if (angle >= Mathf.PI * 0.75f)
		{
			return 3;
		}
		else if (angle > Mathf.PI * 0.25f)
		{
			return 0;
		}
		else if (angle >= Mathf.PI * -0.25f)
		{
			return 1;
		}
		else if (angle > Mathf.PI * -0.75f)
		{
			return 2;
		}
		else
		{
			return 3;
		}
	}

	public void Stop()
	{
		SlowDown();
		Rb.velocity = Vector2.zero;
	}

	public void SlowDown()
	{
		moving = false;
		Cac?.SetRunning(false);
	}

	private void SetVelocity(Vector2 velocity) => Rb.velocity = velocity;

	public void KnockBack(Vector3 direction)
	{
		SetVelocity(direction);
	}

	public void PreventMovementInputForDuration(WaitForSeconds wait)
		=> StartCoroutine(StopMovementForDuration(wait));

	private IEnumerator StopMovementForDuration(WaitForSeconds wait)
	{
		CanMove = false;
		yield return wait;
		CanMove = true;
	}

	public void PreventMovementInputUntilConditionIsMet(Func<bool> condition)
		=> StartCoroutine(StopMovementUntil(condition));

	private IEnumerator StopMovementUntil(Func<bool> condition)
	{
		CanMove = false;
		yield return new WaitUntil(condition);
		CanMove = true;
	}

	public Vector3 GetDirection() => direction;

	public Vector3 GetFacingDirection() => DirectionFloatValue;

	public void FaceDirection(Vector3 direction)
	{
		direction.Normalize();
		this.direction = direction;
		directionID = ConvertDirectionToInt(direction);
		Cac?.SetDirection(directionID);
	}
}
