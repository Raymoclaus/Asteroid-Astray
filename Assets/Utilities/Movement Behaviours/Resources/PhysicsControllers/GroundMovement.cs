using UnityEngine;
using MovementBehaviours;
using System;
using CustomDataTypes;

namespace PhysicsControllers
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class GroundMovement : MonoBehaviour, IPhysicsController
	{
		private Rigidbody2D rb;
		private Rigidbody2D Rb => rb ?? (rb = GetComponent<Rigidbody2D>());
		[SerializeField] private Collider2D col;
		[SerializeField] private Transform selfPivot;
		private Transform SelfPivot => selfPivot != null ? selfPivot : transform;
		[SerializeField] private CharacterAnimationController animController;
		public Vector3 SelfPosition => SelfPivot.position;
		[SerializeField] private float momentumControl = 1f;
		[Range(0f, 1f)]
		[SerializeField] private float stoppingMomentumMultiplier = 0.5f;
		private bool applyingForce = false;
		private bool canMove = true;
		private Vector2 direction = Vector2.down;
		public Direction Facing
		{
			get
			{
				switch (directionID)
				{
					default: return Direction.Up;
					case 0: return Direction.Up;
					case 1: return Direction.Right;
					case 2: return Direction.Down;
					case 3: return Direction.Left;
				}
			}
		}

		private int directionID = 2;
		private string blockMovementTimerID, deactivateColliderTimerID;

		public event Action<bool> OnRunStateChanged;
		public event Action<float> OnDirectionChanged;

		private void Awake()
		{
			blockMovementTimerID = gameObject.GetInstanceID() + "Block Movement Timer";
			TimerTracker.AddTimer(blockMovementTimerID, 0f, null, null);
			deactivateColliderTimerID = gameObject.GetInstanceID() + "Deactivate Collider Timer";
			TimerTracker.AddTimer(deactivateColliderTimerID, 0f, () => EnableCollider = true, null);
		}

		protected virtual void FixedUpdate()
		{
			if (!applyingForce)
			{
				AdjustVelocity(Vector2.zero);
			}
		}

		private void Update()
		{
			float speed = CurrentSpeed;
			if (speed == 0f)
			{
				animController.SetRunning(false);
				animController?.SetSpeedMultiplier(1f);
			}
			else
			{
				animController?.SetSpeedMultiplier(CurrentSpeed);
			}
		}

		private void AdjustVelocity(Vector2 direction)
		{
			float distance = 0f;
			if (direction != Vector2.zero)
			{
				distance = Vector2.Distance(Rb.velocity, direction);
			}
			float delta = Mathf.Lerp(stoppingMomentumMultiplier, 1f, 1f - 1f / distance);
			SetVelocity(Vector2.MoveTowards(
				Rb.velocity, direction, momentumControl * delta / Rb.mass));
		}

		public void MoveAwayFromPosition(Vector3 targetPosition, float speed)
		{
			Vector2 direction = SelfPosition - targetPosition;
			MoveInDirection(direction, speed);
		}

		public void MoveInDirection(Vector3 direction, float speed)
			=> MoveTowardsPosition(SelfPosition + direction.normalized * speed, speed);

		public void MoveTowardsPosition(Vector3 targetPosition, float speed, float smoothingPower = 0f)
		{
			Vector2 direction = targetPosition - SelfPosition;
			float distanceToPosition = direction.magnitude;
			direction.Normalize();
			float distanceSpeedRatio = distanceToPosition / speed;
			float smoothingDelta = Mathf.Pow(Mathf.Clamp01(distanceSpeedRatio), smoothingPower);
			Move(direction * speed * smoothingDelta);
		}

		private void Move(Vector2 direction)
		{
			if (!CanMove) return;

			applyingForce = true;
			animController?.SetRunning(true);
			AdjustVelocity(direction);
			FaceDirection(direction);
			OnRunStateChanged?.Invoke(true);
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
			applyingForce = false;
			OnRunStateChanged?.Invoke(false);
		}

		public void SetVelocity(Vector3 velocity) => Rb.velocity = velocity;

		public bool CanMove
		{
			get => canMove && TimerTracker.GetTimer(blockMovementTimerID) <= 0f;
			set => canMove = value;
		}

		public void PreventMovementInputForDuration(float duration)
		{
			float currentBlockMovementTimer = TimerTracker.GetTimer(blockMovementTimerID);
			float setDuration = Mathf.Max(currentBlockMovementTimer, duration);
			TimerTracker.SetTimer(blockMovementTimerID, setDuration);
		}

		public Vector3 MovementDirection => direction;

		public Vector3 FacingDirection => IntPair.GetDirection(Facing);

		public void FaceDirection(Vector3 direction)
		{
			direction.Normalize();
			this.direction = direction;
			directionID = ConvertDirectionToInt(direction);
			float angle = Vector2.SignedAngle(Vector2.up, direction);
			animController?.SetDirection(angle);
			OnDirectionChanged?.Invoke(angle);
		}

		public bool EnableCollider
		{
			get { return col.enabled; }
			set { col.enabled = value; }
		}

		public Vector3 CurrentVelocity => Rb.velocity;

		public float CurrentSpeed => CurrentVelocity.magnitude;

		public void DeactivateColliderForDuration(float duration)
		{
			EnableCollider = false;
			TimerTracker.SetTimer(deactivateColliderTimerID, duration);
		}
	}

}