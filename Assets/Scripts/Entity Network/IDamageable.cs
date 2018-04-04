using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	bool TakeDamage(float damage, Vector2 damagePos);
}