using UnityEngine;
using System.Collections.Generic;

public class DrillBit : MonoBehaviour
{
	public Collider2D drillCol;
	public Entity parent;
	private bool isDrilling;
	public bool IsDrilling { get { return isDrilling; } }
	public bool CanDrill { get { return parent.canDrill; } }
	public IDrillableObject drillTarget;
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
	[SerializeField]
	private GameObject drillLaunchSparkEffect;

	void Start ()
	{
		drillCol = drillCol ?? GetComponentInChildren<Collider2D>();
		parent.AttachDrill(this);
	}

	private void Update()
	{
		if (isDrilling)
		{
			InflictDamage();
		}
	}

	private void InflictDamage()
	{
		//query damage from parent
		float damage = parent.DrillDamageQuery(firstHit);
		//bigger effects for more damage
		ResizeParticleSystem(damage * sparkSizeModifier);
		//This is so that no damage is dealt while drilling while game is paused
		if (!firstHit)
		{
			damage *= Time.deltaTime * 60f;
		}
		firstHit = false;
		//if damage is 0 then stop drilling
		if (damage <= 0f && !Pause.IsPaused && !Pause.isShifting)
		{
			bool launch =
				InputHandler.GetInputUp("Stop") > 0f &&
				parent == Shuttle.singleton &&
				Shuttle.singleton.ShouldLaunch();
			Vector2 launchDirection = Vector2.up;
			if (launch)
			{
				launchDirection = Shuttle.LaunchDirection(((Entity)drillTarget).transform);
				Transform eff = Instantiate(drillLaunchSparkEffect).transform;
				eff.parent = ParticleGenerator.holder;
				eff.position = transform.position;
				float angle = Vector2.SignedAngle(Vector2.up, launchDirection);
				eff.eulerAngles = Vector3.forward * angle;
				Pause.TemporaryPause();
				CameraCtrl.CamShake();
				CameraCtrl.QuickZoom();
				ScreenRippleEffectController.StartRipple(wait: 0.5f);
				Shuttle.singleton.DrillLaunchArcDisable();
			}
			StopDrilling(launch, launchDirection, Shuttle.singleton);
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
		isDrilling = true;
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
		isDrilling = false;
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
	}

	public bool Verify(Entity target)
	{
		return parent.VerifyDrillTarget(target);
	}

	private void TriggerParticleEffects(bool start)
	{
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
		foreach (ParticleSystem ps in DrillSparks)
		{
			ParticleSystem.MainModule main = ps.main;
			main.startSpeed = size;
		}
	}
}
