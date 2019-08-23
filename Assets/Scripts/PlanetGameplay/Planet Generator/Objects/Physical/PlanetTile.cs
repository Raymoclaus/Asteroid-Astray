using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlanetTile : PlanetRoomObject
{
	private SpriteRenderer sprRend;
	public SpriteRenderer SprRend => sprRend ?? (sprRend = GetComponent<SpriteRenderer>());
}
