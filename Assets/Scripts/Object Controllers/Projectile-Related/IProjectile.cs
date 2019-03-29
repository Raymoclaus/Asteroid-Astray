using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
	void Hit(Entity obj, Vector2 contactPoint);
	Entity GetShooter();
}