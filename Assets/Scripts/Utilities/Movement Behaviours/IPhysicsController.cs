using UnityEngine;

namespace MovementBehaviours
{
	public interface IPhysicsController
	{
		void MoveInDirection(Vector3 direction, float speed);
		void MoveTowardsPosition(Vector3 position, float speed, float smoothingPower);
		void MoveAwayFromPosition(Vector3 position, float speed);
		void Stop();
		void SlowDown();
		Vector3 SelfPosition { get; }
		void SetVelocity(Vector3 direction);
		bool CanMove { get; }
		void PreventMovementInputForDuration(float duration);
		Vector3 MovementDirection { get; }
		Vector3 FacingDirection { get; }
		void FaceDirection(Vector3 direction);
		bool EnableCollider { get; set; }
		void DeactivateColliderForDuration(float duration);
	}

}