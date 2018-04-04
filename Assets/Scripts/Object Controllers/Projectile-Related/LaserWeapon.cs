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
	private float cooldown = 0.5f;
	[SerializeField]
	private Collider2D[] parentColliders;


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
		foreach (Transform weapon in weapons)
		{
			LaserBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapon.position, transform.rotation, weapon.GetChild(0).position - weapon.position,
				laserTarget.position - weapon.position, pool, weapon, parentColliders);
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