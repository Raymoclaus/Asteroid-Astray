using UnityEngine;
using System.Collections.Generic;

public class EntityPrefabController : MonoBehaviour
{
	[Header("If a space-priority entity is spawned in a chunk then no other entities will spawn in the same chunk.")]
	public List<SpawnableEntity> spacePriorityEntities;
	[Header("These entities will allow other entity types to be spawned in the same chunk.")]
	public List<SpawnableEntity> spawnableEntities;
}