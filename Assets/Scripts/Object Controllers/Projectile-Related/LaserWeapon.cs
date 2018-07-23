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
	private const int poolReserve = 20;
	private List<LaserBlast> pool = new List<LaserBlast>(poolReserve);
	private float timer = 0f;
	[SerializeField]
	private float cooldown = 0.5f, recoil = 1f, stoppingForce = 0.2f;
	[SerializeField]
	private Entity parent;
	[SerializeField]
	private GameObject muzzleFlash;


	private void Awake()
	{
		blastPoolHolder = new GameObject("Blast Pool Holder").transform;
		blastPoolHolder.parent = ParticleGenerator.singleton.transform;
		FillPool();
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		}

		if (parent == Shuttle.singleton)
		{
			if (Input.GetKey(KeyCode.Space))
			{
				Fire();
			}
		}
	}

	private void Fire()
	{
		if (timer <= 0f)
		{
			timer = cooldown;
		}
		else return;

		float angle = Mathf.Deg2Rad * -parent.transform.eulerAngles.z;

		if (parent.Rb != null)
		{
			parent.Rb.velocity *= stoppingForce;
			Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			dir.Normalize();
			parent.Rb.AddForce(-dir * recoil);
		}

		bool flipMuzzleFlash = true;
		foreach (Transform weapon in weapons)
		{
			LaserBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapon.position, transform.rotation, weapon.GetChild(0).position - weapon.position,
				laserTarget.position - weapon.position, pool, weapon, parent);
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
			LaserBlast newObj = Instantiate(blastPrefab, blastPoolHolder);
			newObj.gameObject.SetActive(false);
			pool.Add(newObj);
		}
	}

	private void OnDestroy()
	{
		if (blastPoolHolder == null) return;
		Destroy(blastPoolHolder.gameObject);
	}
}