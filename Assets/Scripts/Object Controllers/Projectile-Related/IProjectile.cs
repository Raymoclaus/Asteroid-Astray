using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
	void Hit(IDamageable obj, Vector2 contactPoint);
	Entity GetShooter();
}