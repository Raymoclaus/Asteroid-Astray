using System.Collections.Generic;
using UnityEngine;

public class StraightWeapon : MonoBehaviour
{
	[SerializeField]
	private Transform[] weapons;
	private Transform blastPoolHolder;
	[SerializeField]
	private StraightBlast blastPrefab;
	private const int poolReserve = 50;
	private List<StraightBlast> pool = new List<StraightBlast>(poolReserve);
	private float timer = 0f;
	[SerializeField]
	private bool alternatingFire = true;
	private int nextWeaponCounter = 0;
	[SerializeField]
	private float cooldown = 0.5f, recoil = 1f;
	[SerializeField]
	private Entity parent;
	[SerializeField]
	private GameObject muzzleFlash;
	[SerializeField]
	private float muzzleFlashAngle = 45f;


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

		if (parent == Shuttle.singleton)
		{
			if (Input.GetKey(KeyCode.Space))
			{
				Fire();
			}
		}
	}

	public void Fire()
	{
		if (timer <= 0f)
		{
			timer = cooldown;
		}
		else return;

		float angle = Mathf.Deg2Rad * -parent.transform.eulerAngles.z;

		if (parent.Rb != null)
		{
			Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			dir.Normalize();
			parent.Rb.AddForce(-dir * recoil);
		} 

		bool flipMuzzleFlash = true;
		for (int i = alternatingFire ? nextWeaponCounter : 0; i < weapons.Length; i++)
		{
			StraightBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapons[i].position, transform.rotation, pool, weapons[i], parent);
			//muzzle flash
			GameObject muzFlash = Instantiate(muzzleFlash);
			muzFlash.transform.position = weapons[i].position;
			muzFlash.transform.eulerAngles = Vector3.forward * (-angle + muzzleFlashAngle) * Mathf.Rad2Deg;
			muzFlash.GetComponent<SpriteRenderer>().flipX = flipMuzzleFlash;
			flipMuzzleFlash = !flipMuzzleFlash;

			if (alternatingFire)
			{
				nextWeaponCounter = (nextWeaponCounter + 1) % weapons.Length;
				break;
			}
		}
	}

	private void FillPool()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			StraightBlast newObj = Instantiate(blastPrefab, blastPoolHolder);
			newObj.gameObject.SetActive(false);
			pool.Add(newObj);
		}
	}
}