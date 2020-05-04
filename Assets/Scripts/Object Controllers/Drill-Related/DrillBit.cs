﻿using UnityEngine;
using System.Collections.Generic;
using EquipmentSystem;

public class DrillBit : MonoBehaviour, IEquipment
{
	public Collider2D drillCol;
	public Character parent;
	public bool IsDrilling { get; private set; }
	public bool CanDrill => parent.CanDrill();
	public bool CanLaunch => parent.CanDrillLaunch;
	public Entity drillTarget;
	private bool firstHit = false;
	public List<ParticleSystem> DrillSparks;
	public float sparkSizeModifier = 20f;
	public Animator drillAnim;

	public AudioSource drillSoundSource;
	public Vector2 drillPitchRange;
	public float pitchModifier = 0.1f;
	public float maxVolume = 1f;
	public float volumeIncrease = 0.1f;
	private float currentVolume;
	[SerializeField] private GameObject[] drillSparkObjects;
	[SerializeField] private float[] drillSparkObjectThresholds = { 0f, 0.5f };
	[SerializeField] private GameObject drillLaunchSparkEffect;
	[SerializeField] private GameObject[] drillLaunchBurstAnimations;

	void Start ()
	{
		drillCol = drillCol ?? GetComponentInChildren<Collider2D>();
		parent.AttachDrill(this);
	}

	private void Update()
	{
		if (IsDrilling)
		{
			InflictDamage();
		}
	}

	private void InflictDamage()
	{
		if (drillTarget == null)
		{
			StopDrilling(true);
			IsDrilling = false;
			return;
		}

		//query damage from parent
		float damage = parent.DrillDamageQuery(firstHit);
		//bigger effects for more damage
		ResizeParticleSystem(firstHit ? 1f : damage / parent.MaxDrillDamage());
		//This is so that no damage is dealt while drilling while game is paused
		if (!firstHit)
		{
			damage *= Time.deltaTime * 60f;
		}
		firstHit = false;
		//if damage is 0 then stop drilling
		if (damage <= 0f && !TimeController.IsStopped)
		{
			bool launch = parent.ShouldLaunch() && drillTarget.CanBeLaunched();
			Vector2 launchDirection = Vector2.up;
			if (launch)
			{
				launchDirection = parent.LaunchDirection(((Entity)drillTarget).transform);
				Transform eff = Instantiate(drillLaunchSparkEffect).transform;
				eff.parent = ParticleGenerator.holder;
				eff.position = transform.position;
				float angle = -Vector2.SignedAngle(Vector2.up, launchDirection);
				eff.eulerAngles = Vector3.forward * -angle;
				parent.Launching();
				if (drillLaunchBurstAnimations.Length > 0)
				{
					TimeController.DelayedAction(() =>
					{
						int chooseLaunchBurstAnimation = Random.Range(0, drillLaunchBurstAnimations.Length);
						Transform burst = Instantiate(drillLaunchBurstAnimations[chooseLaunchBurstAnimation]).transform;
						burst.parent = ParticleGenerator.holder;
						burst.position = transform.position;
						burst.eulerAngles = Vector3.forward * -angle;
					}, 0.02f, true);
				}
			}
			StopDrilling(false, launch, launchDirection, parent);
		}
		//else send the damage to the drill target
		else
		{
			if (drillTarget.TakeDrillDamage(damage, transform.position, parent, 1f))
			{
				parent.DrillComplete();
			}
		}

		//adjust sound
		if (!TimeController.IsStopped)
		{
			currentVolume = Mathf.MoveTowards(currentVolume, maxVolume, maxVolume * volumeIncrease);
		}
		drillSoundSource.volume = TimeController.IsStopped ? 0f : currentVolume;
		drillSoundSource.pitch = Mathf.MoveTowards(drillPitchRange.x, drillPitchRange.y, damage * pitchModifier);
	}

	public void StartDrilling(Entity newTarget)
	{
		if (parent.IsInViewRange)
		{
			TriggerParticleEffects(true);
		}
		IsDrilling = true;
		drillTarget = newTarget;
		firstHit = true;

		currentVolume = 0f;
		drillSoundSource.volume = 0f;
		drillSoundSource.Play();

		if (drillAnim != null)
		{
			drillAnim.SetBool("Drilling", true);
		}
	}

	public void StopDrilling(bool successful, bool launch = false, Vector2? launchDirection = null, Character launcher = null)
	{
		TriggerParticleEffects(false);
		IsDrilling = false;
		if (drillTarget != null)
		{
			drillTarget.StopDrilling(this);
			parent.StoppedDrilling(successful);
			if (launch)
			{
				drillTarget.Launch((Vector2)launchDirection, launcher);
			}
			drillTarget = null;
		}

		currentVolume = 0f;
		drillSoundSource.volume = 0f;
		drillSoundSource.Stop();

		if (drillAnim != null)
		{
			drillAnim.SetBool("Drilling", false);
		}
	}

	public bool Verify(Entity target)
	{
		if (target == null) return false;
		return parent.VerifyDrillTarget(target);
	}

	private void TriggerParticleEffects(bool start)
	{
		for (int i = 0; i < drillSparkObjects.Length; i++)
		{
			GameObject obj = drillSparkObjects[i];
			if (obj == null) continue;
			obj.SetActive(start);
		}

		for (int i = 0; i < DrillSparks.Count; i++)
		{
			ParticleSystem ps = DrillSparks[i];
			if (start)
			{
				ps.Clear();
				ps.Play();
			}
			else
			{
				ps.Stop();
			}
		}
	}

	private void ResizeParticleSystem(float size)
	{
		for (int i = 0; i < drillSparkObjects.Length; i++)
		{
			GameObject obj = drillSparkObjects[i];
			if (obj == null) continue;
			obj.SetActive(size > drillSparkObjectThresholds[i]);
		}

		for (int i = 0; i < DrillSparks.Count; i++)
		{
			ParticleSystem ps = DrillSparks[i];
			ParticleSystem.MainModule main = ps.main;
			main.startSpeed = size * sparkSizeModifier;
		}
	}
}
