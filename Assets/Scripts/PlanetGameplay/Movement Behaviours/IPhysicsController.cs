using System;
using UnityEngine;

public interface IPhysicsController
{
	void MoveInDirection(Vector3 direction);
	void MoveTowardsPosition(Vector3 position, bool slowDownBeforeReachingPosition);
	void MoveAwayFromPosition(Vector3 position);
	void Stop();
	void SlowDown();
	Vector3 SelfPosition { get; }
	void KnockBack(Vector3 direction);
	void PreventMovementInputForDuration(WaitForSeconds wait);
	void PreventMovementInputUntilConditionIsMet(Func<bool> condition);
	Vector3 GetDirection();
	Vector3 GetFacingDirection();
	void FaceDirection(Vector3 direction);
}
