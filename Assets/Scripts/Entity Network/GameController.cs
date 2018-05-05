using UnityEngine;

/// <inheritdoc />
/// The purpose of this class is to run before everything else and make sure everything is set up before gameplay begins
public class GameController : MonoBehaviour
{
	public static GameController singleton;

	public EntityPrefabController prefabs;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		EntityNetwork.CreateGrid();

		EntityGenerator.SetPrefabs(prefabs);
		EntityGenerator.FillTriggerList();
		StartCoroutine(EntityGenerator.ChunkBatchOrder());
	}
}