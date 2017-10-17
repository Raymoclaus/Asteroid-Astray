using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillableObject : MonoBehaviour
{
	//bool is to signal whether the object will be destroyed when taking this damage or not
	public virtual bool TakeDrillDamage(float drillDmg)
	{
		return false;
	}
}
