using System.Collections.Generic;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
	private static LoadingController instance;
	
	private List<bool> loadingReady = new List<bool>();
	[SerializeField] private GameObject holder;
	private bool finishedLoading = false;
	public static bool IsLoading { get { return !instance?.finishedLoading ?? true; } }

	public delegate void LoadingCompleteEventHandler();
	private static event LoadingCompleteEventHandler OnLoadingComplete;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		holder.SetActive(true);
		AddListener(() =>
		{
			holder.SetActive(false);
		});

		loadingReady.Add(false);
		SceneryController.AddListener(() =>
		{
			Ready(0);
		});

		loadingReady.Add(false);
		EntityNetwork.AddListener(() =>
		{
			Ready(1);
		});
		
		loadingReady.Add(false);
		EntityGenerator.AddListener(() =>
		{
			Ready(2);
		});
	}

	public static void AddListener(System.Action action)
	{
		if (!IsLoading)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnLoadingComplete += new LoadingCompleteEventHandler(action);
		}
	}

	private void Ready(int index)
	{
		if (index >= 0 && index < loadingReady.Count)
		{
			loadingReady[index] = true;
		}

		if (AllEssentialSystemsReady())
		{
			OnLoadingComplete?.Invoke();
			OnLoadingComplete = null;
		}
	}

	private bool AllEssentialSystemsReady()
	{
		for (int i = 0; i < loadingReady.Count; i++)
		{
			bool b = loadingReady[i];
			if (!b) return false;
		}
		finishedLoading = true;
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
