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
		if (sceneryController != null)
		{
			OnLoadingComplete.AddEventToWaitFor(sceneryController.OnStarFieldCreated);
		}

		EntityGenerator entityGenerator = FindObjectOfType<EntityGenerator>();
		if (entityGenerator != null)
		{
			OnLoadingComplete.AddEventToWaitFor(entityGenerator.OnPrefabsLoaded);
		}

		EntityNetwork entityNetwork = FindObjectOfType<EntityNetwork>();
		if (entityNetwork != null)
		{
			OnLoadingComplete.AddEventToWaitFor(entityNetwork.OnLoaded);
		}

		NarrativeManager narrativeManager = FindObjectOfType<NarrativeManager>();
		if (narrativeManager != null)
		{
			OnLoadingComplete.AddEventToWaitFor(narrativeManager.OnLoaded);
		}

		OnLoadingComplete.Start();
		OnLoadingComplete.RunWhenReady(() => holder.SetActive(false));
	}
}
