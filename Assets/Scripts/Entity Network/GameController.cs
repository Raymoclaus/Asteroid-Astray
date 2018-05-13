using UnityEngine;
using System.Collections.Generic;

/// <inheritdoc />
/// The purpose of this class is to run before everything else and make sure everything is set up before gameplay begins
public class GameController : MonoBehaviour
{
	public static GameController singleton;
	public EntityPrefabController prefabs;
	public static bool loading = true;
	public GameObject loadingUI;
	[SerializeField]
	private List<GameObject> objsToActivate;
	[SerializeField]
	private List<ChunkFiller> fillersToActivate;

	//booleans to check when certain systems are ready
	private static bool gridCreated, triggerListFilled, entityPrefabsReady, starsGenerated;

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

		loadingUI.SetActive(true);

		StartCoroutine(EntityNetwork.CreateGrid(() =>
		{
			gridCreated = true;
			Ready();
		}));

		StartCoroutine(SceneryController.CreateStarSystems(() =>
		{
			starsGenerated = true;
			Ready();
		}));
	}

	private void Ready()
	{
		if (starsGenerated)
		{
			if (!triggerListFilled)
			{
				StartCoroutine(EntityGenerator.FillTriggerList(() =>
				{
					triggerListFilled = true;
					Ready();
				}));
			}

			if (!entityPrefabsReady)
			{
				StartCoroutine(EntityGenerator.SetPrefabs(prefabs, () =>
				{
					entityPrefabsReady = true;
					Ready();
				}));
			}
		}

		if (gridCreated && triggerListFilled && entityPrefabsReady)
		{
			StartCoroutine(EntityGenerator.ChunkBatchOrder());
			ActivateObjectList();
		}

		if (AllEssentialSystemsReady())
		{
			loadingUI.SetActive(false);
		}
	}

	private bool AllEssentialSystemsReady()
	{
		return gridCreated && triggerListFilled && entityPrefabsReady && starsGenerated;
	}

	private void ActivateObjectList()
	{
		foreach (GameObject obj in objsToActivate)
		{
			obj.SetActive(true);
		}
		foreach (ChunkFiller cf in fillersToActivate)
		{
			cf.enabled = true;
		}
	}
}