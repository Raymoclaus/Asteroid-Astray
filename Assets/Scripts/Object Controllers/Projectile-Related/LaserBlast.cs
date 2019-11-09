using System;
using System.Collections.Generic;
using AttackData;
using AudioUtilities;
using TriggerSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserBlast : MonoBehaviour, IProjectile, IAttackActor
{
	private double sharedID = -1;
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
	protected static AudioManager audioMngr;

	public event Action<IActor> OnDisabled;

	private void OnDisable() => OnDisabled?.Invoke(this);

	protected static AudioManager AudioMngr
		=> audioMngr ?? (audioMngr = FindObjectOfType<AudioManager>());

	public Vector3 FacingDirection => direction;

	public bool CanTriggerPrompts => false;

	public AttackManager GetAttackManager => GetComponent<AttackManager>();

	public Vector3 Position => transform.position;

	public void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir,
		List<LaserBlast> p, Transform wep, Entity shooter, double ID, LaserWeapon wepSystem)
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
		GetAttackManager.AddAttackComponent<OwnerComponent>(shooter);
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

	public void Hit(Entity obj, Vector2 contactPoint)
	{
		////calculate damage
		//float damageCalc = damage;
		//if (!converged)
		//{
		//	damageCalc *= damageReductionBeforeConverge;
		//}

		////create explosion effect based on damage dealt
		//bool isStrongHit = damageCalc >= damage * 0.9f;
		//float dirToObject = Vector2.SignedAngle(Vector2.up, (Vector2)obj.transform.position - contactPoint);
		//GameObject hitFX = Instantiate(isStrongHit ? strongHit : weakHit);
		//hitFX.transform.position = contactPoint;
		//hitFX.transform.eulerAngles = Vector3.forward * (dirToObject + 180f);
		////play sound effect
		//if (AudioMngr != null)
		//{
		//	AudioMngr.PlaySFX(isStrongHit ? strongHitSound : weakHitSound, contactPoint,
		//	pitch: Random.value * 0.2f + 0.9f);
		//}
		////report damage calculation to the object taking the damage
		//obj.TakeDamage(damageCalc, contactPoint, parent, 1f, true);
		////destroy projectile
		//Dissipate();
	}

	private void Dissipate()
	{
		Pause.DelayedAction(() => particleTrail.Stop(), 0f);
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

	public void EnteredTrigger(ITrigger vTrigger)
	{

	}

	public void ExitedTrigger(ITrigger vTrigger)
	{

	}

	public void Hit(IAttackTrigger trigger)
	{
		Dissipate();
	}
}