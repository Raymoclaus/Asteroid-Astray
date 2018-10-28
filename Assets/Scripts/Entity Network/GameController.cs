using UnityEngine;
using System.Collections.Generic;

/// <inheritdoc />
/// The purpose of this class is to run before everything else and make sure everything is set up before gameplay begins
public class GameController : MonoBehaviour
{
	public static GameController singleton;
	public EntityPrefabController prefabs;
	public static bool IsLoading
	{
		get { return singleton.loadingUI.activeSelf; }
	}
	public GameObject loadingUI;
	[SerializeField]
	private List<GameObject> objsToActivate;
	[SerializeField]
	private List<ChunkFiller> fillersToActivate;
	[SerializeField]
	private SceneryController sceneryCtrl;

	//booleans to check when certain systems are ready
	private static bool gridCreated, triggerListFilled, entityPrefabsReady, starsGenerated;
	private List<bool> loadingReady = new List<bool>();

	[SerializeField]
	private bool recordingMode = false;
	public static bool RecordingMode { get { return singleton.recordingMode; } }
	private bool wasRecordingMode;
	public static float UnscaledDeltaTime { get { return RecordingMode ? 1.4f / 60f : Time.unscaledDeltaTime; } }
	[Header("Items to adjust when in recording mode.")]
	#region Recording Mode items to fix
	[SerializeField]
	private AnimationClip drillLaunchLightningEffect;
	#endregion

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

		List<System.Action> preLoadActions = new List<System.Action>();

		preLoadActions.Add(() =>
		{
			loadingReady.Add(false);
			int ID = loadingReady.Count - 1;
			StartCoroutine(EntityNetwork.CreateGrid(() =>
			{
				loadingReady[ID] = true;
				Ready();
				print(ID);
			}));
		});

		preLoadActions.Add(() =>
		{
			loadingReady.Add(false);
			int ID = loadingReady.Count - 1;
			StartCoroutine(sceneryCtrl.CreateStarSystems(() =>
			{
				loadingReady[ID] = true;
				Ready();
				print(ID);
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
				print(ID);
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
				print(ID);
			}));
		});

		foreach (System.Action a in preLoadActions)
		{
			a();
		}
	}

	private void Update()
	{
		UpdateRecordModeFixes();
	}

	private void Ready()
	{
		if (AllEssentialSystemsReady())
		{
			loadingReady = null;
			StartCoroutine(EntityGenerator.ChunkBatchOrder());
			ActivateObjectList();
			loadingUI.SetActive(false);
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

	private void UpdateRecordModeFixes()
	{
		if (wasRecordingMode != recordingMode)
		{
			drillLaunchLightningEffect.frameRate = recordingMode ? 24f * (1f / Time.deltaTime) / 60f : 24f;
			wasRecordingMode = recordingMode;
		}
	}
}