using UnityEngine;
using System.Collections.Generic;

public class DrillBit : MonoBehaviour
{
	public Collider2D drillCol;
	public Entity parent;
	public bool IsDrilling { get; private set; }
	public bool CanDrill { get { return parent.canDrill; } }
	public bool CanLaunch { get { return parent.canDrillLaunch; } }
	public IDrillableObject drillTarget;
	private bool firstHit = false;
	public List<ParticleSystem> DrillSparks;
	public float sparkSizeModifier = 20f;
	public Animator drillAnim;
	[SerializeField]
	private CameraCtrl cameraCtrl;

	public AudioSource drillSoundSource;
	public Vector2 drillPitchRange;
	public float pitchModifier = 0.1f;
	public float maxVolume = 1f;
	public float volumeIncrease = 0.1f;
	private float currentVolume;
	[SerializeField]
	private GameObject[] drillSparkObjects;
	[SerializeField]
	private float[] drillSparkObjectThresholds = { 0f, 0.5f };
	[SerializeField]
	private GameObject drillLaunchSparkEffect;
	[SerializeField]
	private float drillLaunchPauseTime = 0.375f;
	[SerializeField]
	private GameObject[] drillLaunchBurstAnimations;

	void Start ()
	{
		drillCol = drillCol ?? GetComponentInChildren<Collider2D>();
		parent.AttachDrill(this);
		cameraCtrl = cameraCtrl ?? Camera.main.GetComponent<CameraCtrl>();
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
		if (damage <= 0f && !Pause.IsPaused && !Pause.isShifting)
		{
			bool launch = parent.ShouldLaunch();
			Vector2 launchDirection = Vector2.up;
			if (launch)
			{
				launchDirection = parent.LaunchDirection(((Entity)drillTarget).transform);
				Transform eff = Instantiate(drillLaunchSparkEffect).transform;
				eff.parent = ParticleGenerator.holder;
				eff.position = transform.position;
				float angle = -Vector2.SignedAngle(Vector2.up, launchDirection);
				eff.eulerAngles = Vector3.forward * -angle;
				Pause.TemporaryPause(drillLaunchPauseTime);
				if (cameraCtrl)
				{
					cameraCtrl.CamShake();
					cameraCtrl.QuickZoom(0.8f, drillLaunchPauseTime, true);
				}
				Pause.DelayedAction(() =>
				{
					ScreenRippleEffectController.StartRipple(wait: drillLaunchPauseTime);
				}, 0.02f, true);
				parent.Launching();
				if (drillLaunchBurstAnimations.Length > 0)
				{
					Pause.DelayedAction(() =>
					{
						int chooseLaunchBurstAnimation = Random.Range(0, drillLaunchBurstAnimations.Length);
						Transform burst = Instantiate(drillLaunchBurstAnimations[chooseLaunchBurstAnimation]).transform;
						burst.parent = ParticleGenerator.holder;
						burst.position = transform.position;
						burst.eulerAngles = Vector3.forward * -angle;
					}, 0.02f, true);
				}
			}
			StopDrilling(launch, launchDirection, parent);
		}
		//else send the damage to the drill target
		else
		{
			if (drillTarget.TakeDrillDamage(damage, transform.position, parent))
			{
				parent.DrillComplete();
			}
		}

		//adjust sound
		if (!Pause.IsPaused)
		{
			currentVolume = Mathf.MoveTowards(currentVolume, maxVolume, maxVolume * volumeIncrease);
		}
		drillSoundSource.volume = Pause.IsPaused ? 0f : currentVolume;
		drillSoundSource.pitch = Mathf.MoveTowards(drillPitchRange.x, drillPitchRange.y, damage * pitchModifier);
	}

	public void StartDrilling(IDrillableObject newTarget)
	{
		if (parent.isActive)
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

	public void StopDrilling(bool launch = false, Vector2? launchDirection = null, Entity launcher = null)
	{
		TriggerParticleEffects(false);
		IsDrilling = false;
		if (drillTarget != null)
		{
			drillTarget.StopDrilling();
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

		parent.StoppedDrilling();
	}

	public bool Verify(Entity target)
	{
		if (target == null) return false;
		return parent.VerifyDrillTarget(target);
	}

	private void TriggerParticleEffects(bool start)
	{
		foreach (GameObject obj in drillSparkObjects)
		{
			if (obj == null) continue;
			obj.SetActive(start);
		}
		foreach (ParticleSystem ps in DrillSparks)
		{
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
		foreach (ParticleSystem ps in DrillSparks)
		{
			ParticleSystem.MainModule main = ps.main;
			main.startSpeed = size * sparkSizeModifier;
		}
	}
}
