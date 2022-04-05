using StatisticsTracker;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
	[SerializeField] private GameObject holder;

	public OneShotEventGroupWait OnLoadingComplete = new OneShotEventGroupWait(false,
		UniqueIDGenerator.OnLoaded);

	private void Awake()
	{
		holder.SetActive(true);
		UniqueIDGenerator.Load();
		StatisticsIO.Load();

		SceneryController sceneryController = FindObjectOfType<SceneryController>();
		if (sceneryController != null && sceneryController.enabled)
		{
			OnLoadingComplete.AddEventToWaitFor(sceneryController.OnStarFieldCreated);
		}

		EntityGenerator entityGenerator = FindObjectOfType<EntityGenerator>();
		if (entityGenerator != null && entityGenerator.enabled)
		{
			OnLoadingComplete.AddEventToWaitFor(entityGenerator.OnPrefabsLoaded);
		}

		EntityNetwork entityNetwork = FindObjectOfType<EntityNetwork>();
		if (entityNetwork != null && entityNetwork.enabled)
		{
			OnLoadingComplete.AddEventToWaitFor(entityNetwork.OnLoaded);
		}

		NarrativeManager narrativeManager = FindObjectOfType<NarrativeManager>();
		if (narrativeManager != null && narrativeManager.enabled)
		{
			OnLoadingComplete.AddEventToWaitFor(narrativeManager.OnLoaded);
		}

		OnLoadingComplete.Start();
		OnLoadingComplete.RunWhenReady(() => holder.SetActive(false));
	}
}
