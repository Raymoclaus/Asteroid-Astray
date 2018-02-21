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
		firstHit = false;
		//if damage is 0 then stop drilling
		if (damage <= 0f)
		{
			StopDrilling();
		}
		//else send the damage to the drill target
		else
		{
			drillTarget.TakeDrillDamage(damage * Cnsts.TIME_SPEED);
		}
		//bigger effects for more damage
		ResizeParticleSystem(damage * sparkSizeModifier);
	}

	public void StartDrilling(IDrillableObject newTarget)
	{
		TriggerParticleEffects(true);
		isDrilling = true;
		drillTarget = newTarget;
		firstHit = true;
	}

	public void StopDrilling()
	{
		TriggerParticleEffects(false);
		isDrilling = false;
		drillTarget.StopDrilling();
		drillTarget = null;
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
