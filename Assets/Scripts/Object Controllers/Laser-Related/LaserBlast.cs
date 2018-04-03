using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserBlast : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D rb;
	private List<LaserBlast> pool;
	private Transform weapon;
	private const int maxRange = 10;
	[SerializeField]
	private float speed = 10f;
	private Vector2 direction;
	public float inwardMomentumMultiplier = 2f;
	private int solidLayer;
	private Vector2 vel;
	private Collider2D[] excludedColliders;

	private void Awake()
	{
		solidLayer = LayerMask.NameToLayer("Solid");
	}

	private void Update()
	{
		if (Vector2.Distance(transform.position, weapon.position) > maxRange)
		{
			Dissipate();
		}

		SetVelocity();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!CollisionIsValid(collision)) return;

		Dissipate();

		if (collision.gameObject != Shuttle.singleton.Col[0].gameObject)
		{
		}
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
		vel += direction;
		vel.Normalize();
		vel *= speed;
		rb.velocity = vel;
	}

	public void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir, List<LaserBlast> p, Transform wep, Collider2D[] exclude)
	{
		transform.position = startPos;
		transform.rotation = startRot;
		vel = startingDir * speed;
		followDir.Normalize();
		direction = followDir * inwardMomentumMultiplier;
		pool = p;
		weapon = wep;
		excludedColliders = exclude;

		gameObject.SetActive(true);
	}

	private void Dissipate()
	{
		pool.Add(this);
		gameObject.SetActive(false);
	}
}