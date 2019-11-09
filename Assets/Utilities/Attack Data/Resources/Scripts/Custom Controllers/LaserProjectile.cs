using AudioUtilities;
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

		[SerializeField] private ParticleSystem particleTrail;
		[SerializeField] private SpriteRenderer sprRend;
		[SerializeField] private Sprite nonBoostedBullet, boostedBullet;
		[SerializeField] private GameObject weakHitEffect, strongHitEffect;
		[SerializeField] private AudioSO weakHitSound, strongHitSound, sonicBoomSound;
		[SerializeField] private GameObject sonicBoomEffect;

		private static AudioManager audioMngr;
		private static AudioManager AudioMngr =>
			audioMngr != null
				? audioMngr
				: (audioMngr = FindObjectOfType<AudioManager>());

		protected override void DestroySelf()
		{
			PlayHitEffects();
			base.DestroySelf();
		}

		private void PlayHitEffects()
		{
			Vector3 pos = CurrentPosition;

			//visual effects
			GameObject hitEffect = Converged ? strongHitEffect : weakHitEffect;
			GameObject copy = Instantiate(hitEffect, null);
			copy.transform.position = pos;
			copy.transform.eulerAngles = Vector3.forward * (transform.eulerAngles.z + 180f);
			ParticleSystem ps = Instantiate(particleTrail, pos, transform.rotation, transform);
			particleTrail.transform.parent = null;
			particleTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

			//sound effects
			AudioSO sound = Converged ? strongHitSound : weakHitSound;
			AudioMngr.PlaySFX(sound, pos, null);

			//reset
			sprRend.sprite = nonBoostedBullet;
		}

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

			Vector3 pos = CurrentPosition;

			Damage = baseDamageAfterConverge;
			Vector3 boostedVelocity = InitialWeaponDirection * boostedSpeed;
			Velocity = boostedVelocity;
			Vector3 convergePos = TargetConvergeLocation;
			Position = convergePos;

			Instantiate(sonicBoomEffect, convergePos, transform.rotation, null);
			AudioMngr.PlaySFX(sonicBoomSound, convergePos, null);

			sprRend.sprite = boostedBullet;
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

		protected override Vector3 Velocity
		{
			get => base.Velocity;
			set
			{
				base.Velocity = value;
				float angle = Vector2.SignedAngle(Vector2.up, value);
				transform.eulerAngles = Vector3.forward * angle;
			}
		}
	} 
}
