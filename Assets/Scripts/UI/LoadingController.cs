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
	private List<System.Action> postLoadActions = new List<System.Action>();

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

		for (int i = 0; i < preLoadActions.Count; i++)
		{
			System.Action a = preLoadActions[i];
			a?.Invoke();
		}
	}

	private void Ready()
	{
		if (AllEssentialSystemsReady())
		{
			Debug.Log("Finished Loading");
			loadingReady = null;
			EntityNetwork.RunInitialisationActions();
			trackerSO.isLoading = false;
			holder.SetActive(false);
			finishedLoading = true;

			for (int i = 0; i < postLoadActions.Count; i++)
			{
				System.Action a = postLoadActions[i];
				a?.Invoke();
			}
		}
	}

	private bool AllEssentialSystemsReady()
	{
		for (int i = 0; i < loadingReady.Count; i++)
		{
			bool b = loadingReady[i];
			if (!b) return false;
		}
		return true;
	}

	public void AddPostLoadAction(System.Action action)
	{
		if (finishedLoading)
		{
			action?.Invoke();
		}
		else
		{
			postLoadActions.Add(action);
		}
	}
}
