using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class EntityPrefabLoader
{
	public static List<SpawnableEntity> spawnableEntities = new List<SpawnableEntity>();
	public static event Action OnPrefabsLoaded;
	public static bool IsReady { get; set; }

	public static SpawnableEntity GetSpawnableEntityByFileName(string filename)
	{
		for (int i = 0; i < spawnableEntities.Count; i++)
		{
			if (spawnableEntities[i].prefab.name == filename) return spawnableEntities[i];
		}

		return null;
	}

	public static void LoadPrefabs()
	{
		if (IsReady)
		{
			OnPrefabsLoaded?.Invoke();
			return;
		}

		AsyncOperationHandle<IList<SpawnableEntity>> handle
			= Addressables.LoadAssetsAsync<SpawnableEntity>("Spawnable Entities", null);
		handle.Completed += SetPrefabs;
	}

	private static void SetPrefabs(AsyncOperationHandle<IList<SpawnableEntity>> handle)
	{
		IList<SpawnableEntity> results = handle.Result;
		if (results == null)
		{
			Debug.Log("No Spawnable Entities Found.");
			return;
		}

		foreach (SpawnableEntity se in results)
		{
			spawnableEntities.Add(se);
		}

		IsReady = true;
		OnPrefabsLoaded?.Invoke();
		OnPrefabsLoaded = null;
	}
}
