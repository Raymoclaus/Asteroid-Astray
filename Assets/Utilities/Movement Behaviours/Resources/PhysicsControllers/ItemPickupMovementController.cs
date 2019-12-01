using MovementBehaviours;
using System;
using UnityEngine;

public class ItemPickupMovementController : MonoBehaviour, IPhysicsController
{
	[SerializeField] private float momentumControl = 0.1f;
	private string blockMovementTimerID;
	private bool moving;
	private bool canMove = true;

	private void Awake()
	{
		blockMovementTimerID = gameObject.GetInstanceID() + "Block Movement Timer";
		TimerTracker.AddTimer(blockMovementTimerID, 0f, null, null);
	}

	private void FixedUpdate()
	{
		if (!moving)
		{
			AdjustVelocity(Vector3.zero);
		}

		transform.position += CurrentVelocity * Time.deltaTime * Time.timeScale;
	}

	private void AdjustVelocity(Vector3 velocity)
	{
		float frameIndependantMomentumControl = momentumControl * Time.deltaTime;
		SetVelocity(Vector3.MoveTowards(CurrentVelocity, velocity,
			frameIndependantMomentumControl));
	}

	public Vector3 SelfPosition => transform.position;

	public bool CanMove
	{
		get => canMove && TimerTracker.GetTimer(blockMovementTimerID) <= 0f;
		set => canMove = value;
	}

	public Vector3 MovementDirection => CurrentVelocity.normalized;

	public Vector3 FacingDirection => CurrentVelocity.normalized;

	public bool EnableCollider { get; set; }

	public Vector3 CurrentVelocity { get; private set; }

	public event Action<bool> OnRunStateChanged;
	public event Action<float> OnDirectionChanged;

	public void DeactivateColliderForDuration(float duration) { }

	public void FaceDirection(Vector3 direction)
		=> OnDirectionChanged?.Invoke(Vector2.Angle(Vector2.up, direction));

	public void MoveAwayFromPosition(Vector3 position, float speed)
	{
		Vector3 direction = (SelfPosition - position).normalized;
		MoveInDirection(direction, speed);
	}

	public void MoveInDirection(Vector3 direction, float speed)
	{
		Vector3 position = SelfPosition + direction.normalized * speed;
		MoveTowardsPosition(position, speed);
	}

	public void MoveTowardsPosition(Vector3 position, float speed, float smoothingPower = 0f)
	{
		Vector2 direction = position - SelfPosition;
		float distanceToPosition = direction.magnitude;
		direction.Normalize();
		float currentSpeed = CurrentVelocity.magnitude;
		float timeAdjustedDistance = distanceToPosition / Time.deltaTime / Time.timeScale;
		float adjustedSpeed = currentSpeed + speed;
		if (timeAdjustedDistance <= adjustedSpeed)
		{
			transform.position = position;
			CurrentVelocity = Vector3.zero;
		}
		else
		{
			Vector2 vector = direction * adjustedSpeed;
			Move(vector);
		}
	}

	private void Move(Vector3 velocity)
	{
		if (!CanMove) return;

		moving = true;
		SetVelocity(velocity);
		FaceDirection(velocity);
		OnRunStateChanged?.Invoke(true);
	}

	public void PreventMovementInputForDuration(float duration)
	{
		float currentBlockMovementTimer = TimerTracker.GetTimer(blockMovementTimerID);
		float setDuration = Mathf.Max(currentBlockMovementTimer, duration);
		TimerTracker.SetTimer(blockMovementTimerID, setDuration);
	}

	public void SetVelocity(Vector3 direction)
	{
		CurrentVelocity = direction;
	}

	public void Stop()
	{
		SlowDown();
		SetVelocity(Vector3.zero);
	}

	public void SlowDown()
	{
		moving = false;
		OnRunStateChanged?.Invoke(false);
	}
}
