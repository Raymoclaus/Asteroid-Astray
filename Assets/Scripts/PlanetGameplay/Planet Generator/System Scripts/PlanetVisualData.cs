﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Objects/Planet/Visual Data")]
public class PlanetVisualData : ScriptableObject
{
	public string type;
	public TileBase wallTile, floorTile, floorDetailTile;
	public float detailChance = 0.2f;
	public List<Sprite> keys, locks;
}
