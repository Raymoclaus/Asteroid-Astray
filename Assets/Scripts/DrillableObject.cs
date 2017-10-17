using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillableObject : MonoBehaviour {
	[Header("Drillable Object properties")]
	protected bool beingDrilled;
	public bool canShake;

	//bool is to signal whether the object will be destroyed when taking this damage or not
	public virtual bool TakeDrillDamage(float drillDmg) {
		Shake();
		beingDrilled = true;
		return false;
	}

	public virtual void EarlyUpdate() { ShakeReset(); }

	public virtual void Shake() {}

	public virtual void ShakeReset() { beingDrilled = false; }
}
