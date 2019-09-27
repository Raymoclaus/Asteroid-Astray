using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpooder : RoomEnemy
{
	public new const float DIFFICULTY_LEVEL = 1f;

	public override float DifficultyModifier => DIFFICULTY_LEVEL;

	public override string ObjectName => "Spooder";

	public override ObjType GetObjectType() => ObjType.Spooder;
}
