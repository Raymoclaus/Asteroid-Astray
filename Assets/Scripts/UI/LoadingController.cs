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
	private GameObject holder;
	public static bool IsLoading { get; private set; } = true;

	public delegate void LoadingCompleteEventHandler();
	public static event LoadingCompleteEventHandler OnLoadingComplete;

	private void Awake()
	{
		holder.SetActive(true);
		IsLoading = true;
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
			holder.SetActive(false);
			IsLoading = false;
			OnLoadingComplete?.Invoke();
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

	public static void AddPostLoadAction(System.Action action)
	{
		if (IsLoading)
		{
			OnLoadingComplete += new LoadingCompleteEventHandler(action);
		}
		else
		{
			action?.Invoke();
		}
	}
}
