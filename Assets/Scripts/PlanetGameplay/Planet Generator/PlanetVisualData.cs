using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Objects/Planet/Visual Data")]
public class PlanetVisualData : ScriptableObject
{
	public AreaType type;
	public TileBase wallTile, floorTile;
	public List<Sprite> keys, locks;
}
