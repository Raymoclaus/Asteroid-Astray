using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserBlast : MonoBehaviour, IProjectile
{
	[SerializeField]
	private Rigidbody2D rb;
	private List<LaserBlast> pool;
	private Transform weapon;
	private const int maxRange = 10;
	[SerializeField]
	private float startSpeed = 5f, minSpeed = 1f, maxSpeed = 10f, boostTime = 0.5f, convergeStrength = 1f;
	private float boostCounter = 0f;
	private Vector2 direction, forward, convergePoint;
	public float inwardMomentumMultiplier = 5f;
	private static int solidLayer;
	private static bool setLayers;
	private Vector2 vel;
	private Collider2D[] excludedColliders;
	[SerializeField]
	private float damage = 200f;
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
	private GameObject sonicBoom;
	[SerializeField]
	private GameObject weakHit;
	[SerializeField]
	private GameObject strongHit;

	public void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir,
		List<LaserBlast> p, Transform wep, Collider2D[] exclude, Entity shooter)
	{
		transform.position = startPos;
		transform.rotation = startRot;
		vel = startingDir * startSpeed;
		followDir.Normalize();
		direction = followDir * inwardMomentumMultiplier;
		pool = p;
		weapon = wep;
		forward = weapon.up;
		excludedColliders = exclude;
		converged = false;
		particleTrail.gameObject.SetActive(true);
		particleTrail.Play();
		particleTrail.transform.parent = transform;
		particleTrail.transform.localPosition = Vector3.zero;
		parent = shooter;

		gameObject.SetActive(true);
	}

	private void Awake()
	{
		GetLayers();
	}

	private void GetLayers()
	{
		if (setLayers) return;
		solidLayer = LayerMask.NameToLayer("Solid");
		setLayers = true;
	}

	private void FixedUpdate()
	{
		if (Vector2.Distance(transform.position, weapon.position) > maxRange)
		{
			Dissipate();
		}

		SetVelocity();
	}

	private bool CollisionIsValid(Collider2D collision)
	{
		if (collision.gameObject.layer != solidLayer) return false;

		foreach (Collider2D col in excludedColliders)
		{
			if (col == collision) return false;
		}

		return true;
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
				GameObject sBoom = Instantiate(sonicBoom, ParticleGenerator.singleton.transform);
				sBoom.transform.position = transform.position;
				sBoom.transform.eulerAngles = Vector3.forward * angle;
			}
			converged = true;
		}

		rb.velocity = vel;
	}

	public void Hit(IDamageable obj)
	{
		float damageCalc = damage;

		if (converged)
		{
			float dist = Mathf.Max(3f, Vector2.Distance(convergePoint, transform.position) * 3f) / 3f;
			damageCalc /= dist;
		}
		else
		{
			float angle = 1f - (Vector2.Angle(vel, direction) / 60f);
			angle *= angle;
			damageCalc *= angle;
		}

		float dirToObject = Vector2.SignedAngle(Vector2.up, obj.GetPosition() - (Vector2)transform.position);
		GameObject hitFX = Instantiate(damageCalc >= damage * 0.9f ? strongHit : weakHit);
		hitFX.transform.position = transform.position;
		hitFX.transform.eulerAngles = Vector3.forward * (dirToObject + 180f);

		obj.TakeDamage(damageCalc, transform.position, parent);
		Dissipate();
	}

	private void Dissipate()
	{
		DelayedAction.Go(() => particleTrail.Stop());
		particleTrail.transform.parent = transform.parent;
		pool.Add(this);
		sprRend.sprite = notBoostedBullet;
		boostCounter = 0f;
		gameObject.SetActive(false);
	}
}