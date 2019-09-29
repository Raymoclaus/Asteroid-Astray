using UnityEngine;

[System.Serializable]
public abstract class RoomEnemy : RoomEntity
{
	public const float DIFFICULTY_LEVEL = Mathf.Infinity;

	public virtual float DifficultyModifier => DIFFICULTY_LEVEL;
}
