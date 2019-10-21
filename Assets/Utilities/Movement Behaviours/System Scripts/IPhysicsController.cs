using System;
using UnityEngine;

namespace MovementBehaviours
{
	public interface IPhysicsController
	{
		event Action<bool> OnRunStateChanged;
		event Action<float> OnDirectionChanged;
		void MoveInDirection(Vector3 direction, float speed);
		void MoveTowardsPosition(Vector3 position, float speed, float smoothingPower);
		void MoveAwayFromPosition(Vector3 position, float speed);
		void Stop();
		void SlowDown();
		Vector3 SelfPosition { get; }
		void SetVelocity(Vector3 direction);
		bool CanMove { get; set; }
		void PreventMovementInputForDuration(float duration);
		Vector3 MovementDirection { get; }
		Vector3 CurrentVelocity { get; }
		Vector3 FacingDirection { get; }
		void FaceDirection(Vector3 direction);
		bool EnableCollider { get; set; }
		void DeactivateColliderForDuration(float duration);
	}

}