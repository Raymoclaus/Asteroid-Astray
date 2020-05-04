﻿using AudioUtilities;
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
	[SerializeField]
	private AudioClip shootSound;
	[SerializeField]
	private GameObject sonicBoom;
	[SerializeField]
	private AudioClip sonicBoomSound;
	private List<double> projectileIDs = new List<double>();
	protected static AudioManager audioMngr;
	protected static AudioManager AudioMngr
		=> audioMngr ?? (audioMngr = FindObjectOfType<AudioManager>());

	private void Awake()
	{
		blastPoolHolder = new GameObject("Blast Pool Holder").transform;
		blastPoolHolder.parent = ParticleGenerator.holder;
		FillPool();
		parent.AttachLaser(true);
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		}

		if (parent.CanFireLaser() && !TimeController.IsStopped)
		{
			Fire();
		}
	}

	private void Fire()
	{
		if (timer <= 0f)
		{
			timer = cooldown;
			if (AudioMngr != null)
			{
				AudioMngr.PlaySFX(shootSound, transform.position, transform, 0.7f);
			}
		}
		else return;

		float angle = Mathf.Deg2Rad * -parent.transform.eulerAngles.z;

		if (parent.rb != null)
		{
			parent.rb.velocity *= stoppingForce;
			Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			dir.Normalize();
			parent.rb.AddForce(-dir * recoil);
		}

		bool flipMuzzleFlash = true;
		for (int i = 0; i < weapons.Length; i++)
		{
			Transform weapon = weapons[i];
			LaserBlast blast = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			blast.Shoot(weapon.position, transform.rotation, weapon.GetChild(0).position - weapon.position,
				laserTarget.position - weapon.position, pool, weapon, parent, (double)TimeController.TimeSinceOpen, this);
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

	public void LaserConvergeEffect(double ID, Vector2 position, float angle)
	{
		for (int i = 0; i < projectileIDs.Count; i++)
		{
			if (projectileIDs[i] == ID)
			{
				projectileIDs.RemoveAt(i);
				return;
			}
		}
		projectileIDs.Clear();
		projectileIDs.Add(ID);
		//sonic boom particle effect
		GameObject sBoom = Instantiate(sonicBoom, ParticleGenerator.holder);
		sBoom.transform.position = position;
		sBoom.transform.eulerAngles = Vector3.forward * angle;
		//play sonic boom sound
		if (AudioMngr != null)
		{
			AudioMngr.PlaySFX(sonicBoomSound, transform.position);
		}
	}
}