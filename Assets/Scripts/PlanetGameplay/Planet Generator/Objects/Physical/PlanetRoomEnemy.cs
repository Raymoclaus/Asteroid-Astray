using UnityEngine;

public abstract class PlanetRoomEnemy : PlanetRoomEntity
{
	protected PlanetPlayer player;
	protected float DistanceToPlayer
		=> player != null ?
		Vector3.Distance(GetPivotPosition(), player.GetPivotPosition())
		: Mathf.Infinity;

	protected override void Awake()
	{
		base.Awake();
		player = FindObjectOfType<PlanetPlayer>();
	}
}
