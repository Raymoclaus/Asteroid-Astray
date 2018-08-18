using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserBlast : MonoBehaviour, IProjectile
{
	private int sharedID = -1;
	private LaserWeapon weaponSystem;
	[SerializeField]
	private Rigidbody2D rb;
	private List<LaserBlast> pool;
	private Transform weapon;
	private Vector2 firingPos;
	private float maxRange = 10f;
	[SerializeField]
	private float startSpeed = 5f, minSpeed = 1f, maxSpeed = 10f, boostTime = 0.5f, convergeStrength = 1f;
	private float boostCounter = 0f;
	private Vector2 direction, forward, convergePoint;
	public float inwardMomentumMultiplier = 5f;
	private Vector2 vel;
	[SerializeField]
	private float damage = 200f;
	[SerializeField]
	private float damageReductionBeforeConverge = 0.2f;
	private bool converged;
	[SerializeField]
	private ParticleSystem particleTrail;
	private Entity parent;
	[SerializeField]
	private Sprite boostedBullet;
	[SerializeField]
	private Sprite notBoostedBullet;
	[SerializeField]
	private SpriteRenderer sprRend;
	[SerializeField]
	private GameObject weakHit;
	[SerializeField]
	private GameObject strongHit;
	[SerializeField]
	private AudioClip strongHitSound;
	[SerializeField]
	private AudioClip weakHitSound;

	public void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir,
		List<LaserBlast> p, Transform wep, Entity shooter, int ID, LaserWeapon wepSystem)
	{
		firingPos = startPos;
		transform.position = startPos;
		transform.rotation = startRot;
		vel = startingDir * startSpeed;
		followDir.Normalize();
		direction = followDir * inwardMomentumMultiplier;
		pool = p;
		weapon = wep;
		forward = weapon.up;
		converged = false;
		particleTrail.gameObject.SetActive(true);
		particleTrail.Play();
		particleTrail.transform.parent = transform;
		particleTrail.transform.localPosition = Vector3.zero;
		parent = shooter;
		sharedID = ID;
		weaponSystem = wepSystem;

		gameObject.SetActive(true);
	}

	private void Update()
	{
		if (Vector2.Distance(transform.position, firingPos) > maxRange)
		{
			Dissipate();
		}
	}

	private void FixedUpdate()
	{
		SetVelocity();
	}

	private void SetVelocity()
	{
		if (boostCounter <= boostTime && !converged)
		{
			float sp = Mathf.Lerp(startSpeed, minSpeed, boostCounter / boostTime);
			boostCounter += Time.deltaTime;

			vel += direction * Time.deltaTime * 60f * convergeStrength;
			vel.Normalize();
			vel *= sp;
		}
		else
		{
			vel = forward * maxSpeed;
			if (!converged)
			{
				convergePoint = transform.position;
				sprRend.sprite = boostedBullet;
				float angle = Vector2.SignedAngle(Vector2.up, vel);
				transform.eulerAngles = Vector3.forward * angle;
				//signal weapon system to create sonic boom effects
				weaponSystem.LaserConvergeEffect(sharedID, convergePoint, angle);
			}
			converged = true;
		}

		rb.velocity = vel;
	}

	public void Hit(IDamageable obj, Vector2 contactPoint)
	{
		//calculate damage
		float damageCalc = damage;
		if (!converged)
		{
			damageCalc *= damageReductionBeforeConverge;
		}

		//create explosion effect based on damage dealt
		bool isStrongHit = damageCalc >= damage * 0.9f;
		float dirToObject = Vector2.SignedAngle(Vector2.up, obj.GetPosition() - contactPoint);
		GameObject hitFX = Instantiate(isStrongHit ? strongHit : weakHit);
		hitFX.transform.position = contactPoint;
		hitFX.transform.eulerAngles = Vector3.forward * (dirToObject + 180f);
		//play sound effect
		AudioManager.PlaySFX(isStrongHit ? strongHitSound : weakHitSound, contactPoint,
			pitch: Random.value * 0.2f + 0.9f);
		//report damage calculation to the object taking the damage
		obj.TakeDamage(damageCalc, contactPoint, parent);
		//destroy projectile
		Dissipate();
	}

	private void Dissipate()
	{
		StartCoroutine(DelayedAction.Go(() => particleTrail.Stop()));
		particleTrail.transform.parent = transform.parent;
		pool.Add(this);
		sprRend.sprite = notBoostedBullet;
		boostCounter = 0f;
		gameObject.SetActive(false);
	}

	public Entity GetShooter()
	{
		return parent;
	}
}