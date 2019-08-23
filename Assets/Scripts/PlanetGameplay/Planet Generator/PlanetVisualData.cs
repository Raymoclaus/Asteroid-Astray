using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Planet/Visual Data")]
public class PlanetVisualData : ScriptableObject
{
	public AreaType type;
	public List<Sprite> floorTiles;
	public List<Sprite> topWallTiles, rightWallTiles, bottomWallTiles, leftWallTiles;
	public List<Sprite> topLeftInnerWallTiles, topLeftOuterWallTiles,
		topRightInnerWallTiles, topRightOuterWallTiles,
		bottomLeftInnerWallTiles, bottomLeftOuterWallTiles,
		bottomRightInnerWallTiles, bottomRightOuterWallTiles;
	public List<Sprite> keys, locks;
}
