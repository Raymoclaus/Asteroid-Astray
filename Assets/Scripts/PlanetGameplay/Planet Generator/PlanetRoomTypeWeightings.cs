using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Planet/Room Type Weightings")]
public class PlanetRoomTypeWeightings : ScriptableObject
{
	public float emptyRoomWeighting,
		puzzleRoomWeighting,
		enemiesRoomWeighting,
		treasureRoomWeighting,
		npcRoomWeighting;
}
