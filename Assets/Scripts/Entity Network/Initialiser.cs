using UnityEngine;

/// <inheritdoc />
/// The purpose of this class is to run before everything else and make sure everything is set up before gameplay begins
public class Initialiser : MonoBehaviour
{
	public EntityPrefabController prefabs;

	private void Awake()
	{
		EntityNetwork.CreateGrid();

		EntityGenerator.SetPrefabs(prefabs);
		EntityGenerator.FillTriggerList();
		StartCoroutine(EntityGenerator.ChunkBatchOrder());
	}
}