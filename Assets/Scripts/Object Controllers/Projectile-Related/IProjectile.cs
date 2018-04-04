using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
	void Shoot(Vector2 startPos, Quaternion startRot, Vector2 startingDir, Vector2 followDir, List<LaserBlast> p, Transform wep, Collider2D[] exclude);
	void Hit(IDamageable obj);
}