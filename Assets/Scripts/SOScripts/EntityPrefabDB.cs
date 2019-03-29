using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EntityPrefabDatabase")]
public class EntityPrefabDB : ScriptableObject
{
	public List<SpawnableEntity> spawnableEntities;

	public SpawnableEntity GetSpawnableEntity(Entity e)
	{
		for (int i = 0; i < spawnableEntities.Count; i++)
		{
			if (spawnableEntities[i].prefab == e) return spawnableEntities[i];
		}
		return null;
	}
}
