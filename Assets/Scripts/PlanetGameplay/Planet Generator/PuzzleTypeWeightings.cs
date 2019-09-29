using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Planet/Puzzle Weightings")]
public class PuzzleTypeWeightings : ScriptableObject
{
	public float randomMazeRoomWeighting,
		randomTileLightRoomWeighting,
		randomBeamRedirectionRoomWeighting,
		randomBlockPushRoomWeighting,
		randomPatternMatchRoomWeighting;
}
