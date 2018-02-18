using UnityEngine;

public class DrillBit : MonoBehaviour
{
	public Collider2D drillCol;
	public Entity parent;
	private bool isDrilling;
	public bool IsDrilling { get { return isDrilling; } }
	public bool CanDrill { get { return parent.canDrill; } }
	public IDrillableObject drillTarget;
	
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
		float damage = parent.DrillDamageQuery();
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
	}

	public void StartDrilling(IDrillableObject newTarget)
	{
		isDrilling = true;
		drillTarget = newTarget;
	}

	public void StopDrilling()
	{
		isDrilling = false;
		drillTarget.StopDrilling();
		drillTarget = null;
	}
}
