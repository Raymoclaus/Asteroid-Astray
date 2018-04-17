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
	private float speed = 10f;
	private Vector2 direction, forward, convergePoint;
	public float inwardMomentumMultiplier = 2f;
	private static int solidLayer;
	private static bool setLayers;
	private Vector2 vel;
	private Collider2D[] excludedColliders;
	[SerializeField]
	private float damage = 200f;
	private bool converged;
	[SerializeField]
	private float convergeAngle = 1f;
	[SerializeField]
	private ParticleSystem particleTrail;
	private Entity parent;

	public void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir,
		List<LaserBlast> p, Transform wep, Collider2D[] exclude, Entity shooter)
	{
		transform.position = startPos;
		transform.rotation = startRot;
		vel = startingDir * speed;
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
		if (Vector2.Angle(vel, direction) > convergeAngle && !converged)
		{
			vel += direction * Time.deltaTime * 60f;
			vel.Normalize();
			vel *= speed;
		}
		else
		{
			vel = forward * speed;
			if (!converged)
			{
				convergePoint = transform.position;
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

		obj.TakeDamage(damageCalc, transform.position, parent);

		Dissipate();
	}

	private void Dissipate()
	{
		DelayedAction.Go(() => particleTrail.Stop());
		particleTrail.transform.parent = transform.parent;
		pool.Add(this);
		gameObject.SetActive(false);
	}
}