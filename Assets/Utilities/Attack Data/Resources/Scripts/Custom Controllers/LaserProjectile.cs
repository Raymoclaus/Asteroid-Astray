using UnityEngine;
using ValueComponents;

namespace AttackData
{
	public class LaserProjectile : AmmunitionAttackManager
	{
		[SerializeField] private float baseDamageAfterConverge;
		[SerializeField] private float convergeDistance = 3f;
		[SerializeField] private RangedFloatComponent convergeTimerComp;
		[SerializeField] private float endSpeed = 1f, boostedSpeed = 10f;

		private void FixedUpdate()
		{
			UpdateVelocity();
		}

		private Vector3 TargetConvergeLocation
			=> InitialWeaponPosition
			   + InitialWeaponDirection.normalized * convergeDistance;

		private Vector3 CurrentPosition
			=> transform.position;

		private Vector3 VectorToConvergeLocation
			=> TargetConvergeLocation - CurrentPosition;

		private Vector3 DirectionToConvergeLocation
			=> VectorToConvergeLocation.normalized;

		private bool Converged { get; set; }

		private void Converge()
		{
			Converged = true;
			Damage = baseDamageAfterConverge;
			Vector3 boostedVelocity = InitialWeaponDirection * boostedSpeed;
			Velocity = boostedVelocity;
			Vector3 convergePos = TargetConvergeLocation;
			Position = convergePos;
		}

		private void UpdateVelocity()
		{
			if (Converged) return;
			convergeTimerComp.AddValue(Time.deltaTime);
			float currentRatio = convergeTimerComp.CurrentRatio;
			Velocity = Vector3.Lerp(Velocity, VectorToConvergeLocation, currentRatio);
			float currentSpeed = CurrentSpeed;
			float minSpeed = Mathf.Lerp(0f, endSpeed, currentRatio);
			if (currentSpeed < minSpeed)
			{
				CurrentSpeed = minSpeed;
			}

			if (currentRatio >= 1f)
			{
				Converge();
			}
		}
	} 
}
