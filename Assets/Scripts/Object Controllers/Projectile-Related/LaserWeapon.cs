using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MonoBehaviour
{
	[SerializeField]
	private Transform[] weapons;
	[SerializeField]
	private Transform laserTarget;
	private Transform blastPoolHolder;
	[SerializeField]
	private LaserBlast blastPrefab;
	private const int poolReserve = 100;
	private List<LaserBlast> pool = new List<LaserBlast>(poolReserve);
	private float timer = 0f;
	[SerializeField]
	private float cooldown = 0.5f, recoil = 1f, stoppingForce = 0.2f;
	[SerializeField]
	private Collider2D[] parentColliders;
	[SerializeField]
	private Entity parent;
	[SerializeField]
	private GameObject muzzleFlash;


	private void Awake()
	{
		blastPoolHolder = new GameObject("Blast Pool Holder").transform;
		FillPool();
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		}
		if (timer <= 0f && Input.GetKey(KeyCode.Space))
		{
			timer = cooldown;
			Fire();
		}
	}

	private void Fire()
	{
		bool flipMuzzleFlash = true;
		float angle = Mathf.Deg2Rad * -parent.transform.eulerAngles.z;

		if (parent.Rb != null)
		{
			parent.Rb.velocity *= stoppingForce;
			Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			dir.Normalize();
			parent.Rb.AddForce(-dir * recoil);
		}

		foreach (Transform weapon in weapons)
		{
			LaserBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapon.position, transform.rotation, weapon.GetChild(0).position - weapon.position,
				laserTarget.position - weapon.position, pool, weapon, parentColliders, parent);
			//muzzle flash
			GameObject muzFlash = Instantiate(muzzleFlash);
			muzFlash.transform.position = weapon.position;
			muzFlash.transform.eulerAngles = Vector3.forward * -angle * Mathf.Rad2Deg;
			muzFlash.GetComponent<SpriteRenderer>().flipX = flipMuzzleFlash;
			flipMuzzleFlash = !flipMuzzleFlash;
		}
	}

	private void FillPool()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			pool.Add(Instantiate(blastPrefab, blastPoolHolder));
		}
	}
}