using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0, bool flash = true);
	Vector2 GetPosition();
}