using UnityEngine;

[System.Serializable]
public abstract class RoomEnemy : RoomEntity
{
	public const float DIFFICULTY_LEVEL = Mathf.Infinity;

	public RoomEnemy(Room room) : base(room) { }

	public RoomEnemy(Room room, string[] lines) : base(room, lines) { }

	public virtual float DifficultyModifier => DIFFICULTY_LEVEL;
}
