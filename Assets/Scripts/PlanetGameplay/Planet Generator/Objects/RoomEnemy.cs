using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomEnemy : RoomEntity
{
	public const float DIFFICULTY_LEVEL = Mathf.Infinity;

	public virtual float DifficultyModifier => DIFFICULTY_LEVEL;
}
