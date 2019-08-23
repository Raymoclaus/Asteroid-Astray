using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlanetWallTile : PlanetTile
{
	private BoxCollider2D col;
	public BoxCollider2D Col => col ?? (col = GetComponent<BoxCollider2D>());
}
