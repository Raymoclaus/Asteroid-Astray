using System;
using System.Collections.Generic;
using InputHandlerSystem;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
	private static LoadingController instance;
	
	private List<bool> loadingReady = new List<bool>();
	[SerializeField] private GameObject holder;
	private bool finishedLoading = false;
	public static bool IsLoading
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<LoadingController>();
			}
			return !instance?.finishedLoading ?? false;
		}
	}

	private static event Action OnLoadingComplete;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		holder.SetActive(true);
		AddListener(() =>
		{
			holder.SetActive(false);
		});

		if (FindObjectOfType<SceneryController>())
		{
			int ID = loadingReady.Count;
			loadingReady.Add(false);
			SceneryController.AddListener(() =>
			{
				Ready(ID);
			});
		}

		if (FindObjectOfType<EntityGenerator>())
		{
			int ID = loadingReady.Count;
			loadingReady.Add(false);
			EntityGenerator.AddListener(() =>
			{
				Ready(ID);
			});
		}

		if (FindObjectOfType<EntityNetwork>())
		{
			int ID = loadingReady.Count;
			loadingReady.Add(false);
			EntityNetwork.AddListener(() =>
			{
				Ready(ID);
			});
		}
	}

	public static void AddListener(Action action)
	{
		if (!IsLoading)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnLoadingComplete += action;
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
}
