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
	private float cooldown = 0.5f, recoil = 10f;
	[SerializeField]
	private Entity parent;
	[SerializeField]
	private GameObject muzzleFlash;
	private float aimThreshold = 16f;
	private bool flipMuzzleFlash = false;
	public Vector2 aim;


	private void Awake()
	{
		blastPoolHolder = new GameObject("Blast Pool Holder").transform;
		blastPoolHolder.parent = ParticleGenerator.holder;
		FillPool();
		parent.AttachStraightWeapon(true);
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		}

		if (parent.CanFireStraightWeapon())
		{
			Fire();
		}
	}

	public void Fire()
	{
		if (timer <= 0f)
		{
			timer = cooldown;
		}
		else return;

		float angle = parent.transform.eulerAngles.z;

		Vector2 aimPos = aim;
		float aimAngle = -Vector2.SignedAngle(Vector2.up, aimPos - (Vector2)transform.position);

		if (Mathf.Abs(Mathf.DeltaAngle(angle, aimAngle)) < aimThreshold)
		{
			angle = aimAngle;
		}

		angle *= Mathf.Deg2Rad;

		if (parent.Rb != null)
		{
			Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			dir.Normalize();
			parent.Rb.AddForce(-dir * recoil);
		}

		for (int i = alternatingFire ? nextWeaponCounter : 0; i < weapons.Length; i++)
		{
			StraightBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapons[i].position, Quaternion.Euler(Vector3.forward * angle * Mathf.Rad2Deg),
				pool, parent);
			//muzzle flash
			GameObject muzFlash = Instantiate(muzzleFlash);
			muzFlash.transform.position = weapons[i].position;
			muzFlash.transform.eulerAngles = Vector3.forward * transform.eulerAngles.z;
			muzFlash.transform.parent = transform;
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

	private void OnDestroy()
	{
		if (blastPoolHolder == null) return;
		Destroy(blastPoolHolder.gameObject);
	}
}