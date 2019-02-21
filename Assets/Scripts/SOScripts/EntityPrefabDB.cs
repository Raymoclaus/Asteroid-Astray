using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EntityPrefabDatabase")]
public class EntityPrefabDB : ScriptableObject
{
	public List<SpawnableEntity> spawnableEntities;
}
