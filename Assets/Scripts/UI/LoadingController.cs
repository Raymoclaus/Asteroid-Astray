using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
	[SerializeField]
	private EntityPrefabDB prefabs;
	[SerializeField]
	private SceneryController sceneryCtrl;
	private List<bool> loadingReady = new List<bool>();
	[SerializeField]
	private LoadingTracker trackerSO;
	[SerializeField]
	private GameObject holder;
	[HideInInspector]
	public bool finishedLoading = false;

	private void Awake()
	{
		holder.SetActive(true);
		trackerSO.isLoading = true;
		List<System.Action> preLoadActions = new List<System.Action>();

		preLoadActions.Add(() =>
		{
			sceneryCtrl = sceneryCtrl ?? FindObjectOfType<SceneryController>();
			if (sceneryCtrl)
			{
				loadingReady.Add(false);
				int ID = loadingReady.Count - 1;
				StartCoroutine(sceneryCtrl.CreateStarSystems(() =>
				{
					loadingReady[ID] = true;
					Ready();
				}));
			}
		});

		preLoadActions.Add(() =>
		{
			loadingReady.Add(false);
			int ID = loadingReady.Count - 1;
			StartCoroutine(EntityNetwork.CreateGrid(() =>
			{
				loadingReady[ID] = true;
				Ready();
			}));
		});

		preLoadActions.Add(() =>
		{
			loadingReady.Add(false);
			int ID = loadingReady.Count - 1;
			StartCoroutine(EntityGenerator.FillTriggerList(() =>
			{
				loadingReady[ID] = true;
				Ready();
			}));
		});

		preLoadActions.Add(() =>
		{
			loadingReady.Add(false);
			int ID = loadingReady.Count - 1;
			StartCoroutine(EntityGenerator.SetPrefabs(prefabs, () =>
			{
				loadingReady[ID] = true;
				Ready();
			}));
		});

		foreach (System.Action a in preLoadActions)
		{
			a();
		}
	}

	private void Ready()
	{
		if (AllEssentialSystemsReady())
		{
			Debug.Log("Finished Loading");
			loadingReady = null;
			StartCoroutine(EntityGenerator.ChunkBatchOrder());
			EntityNetwork.RunInitialisationActions();
			trackerSO.isLoading = false;
			holder.SetActive(false);
			finishedLoading = true;
		}
	}

	private bool AllEssentialSystemsReady()
	{
		foreach (bool b in loadingReady)
		{
			if (!b) return false;
		}
		return true;
	}
}
