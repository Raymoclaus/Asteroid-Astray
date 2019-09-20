using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetRoomEnemy : PlanetRoomEntity
{
	[SerializeField] protected ProjectileAttack attackPrefab;
	protected PlanetPlayer player;
	[SerializeField] protected float attackRange = 2f;
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
