using SaveSystem;
using System.Collections.Generic;
using TriggerSystem;
using UnityEngine;

namespace QuestSystem
{
	public class WaypointManager : MonoBehaviour
	{
		private static WaypointManager m_instance;
		[SerializeField] private VicinityWaypoint vicinityWaypointPrefab;
		[SerializeField] private AttachableWaypoint attachableWaypointPrefab;

		private const string PREFAB_TYPE_VICINITY_WAYPOINT = "Vicinity Waypoint",
			PREFAB_TYPE_ATTACHABLE_WAYPOINT = "Attachable Waypoint";

		//unique ID, waypoint
		private static Dictionary<string, IWaypoint> waypoints = new Dictionary<string, IWaypoint>();
		private static Transform m_waypointParent;

		private static Transform WaypointParent => m_waypointParent != null
			? m_waypointParent
			: (m_waypointParent = new GameObject("Waypoints").transform);

		public static InvocableOneShotEvent OnLoaded = new InvocableOneShotEvent();

		private void Awake()
		{
			if (m_instance == null)
			{
				m_instance = this;
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			Load();
		}

		private void OnDestroy()
		{
			waypoints.Clear();
		}

		/// <summary>
		/// Adds a waypoint to the tracked list of waypoints.
		/// If the waypoint already has an ID that exists in the list, it will not be added, even if the waypoint itself is not in the list.
		/// </summary>
		/// <param name="waypoint"></param>
		public static void AddWaypoint(IWaypoint waypoint)
		{
			UniqueIDGenerator.AddObject(waypoint);
			waypoints.Add(waypoint.UniqueID, waypoint);
		}

		public static void RemoveWaypoint(IWaypoint waypoint)
		{
			RemoveWaypointByID(waypoint.UniqueID);
		}

		public static void RemoveWaypointByID(string ID)
		{
			if (!IDExists(ID)) return;
			waypoints.Remove(ID);
		}

		private static bool IDExists(string ID)
		{
			return ID != null && waypoints.ContainsKey(ID);
		}

		public static IWaypoint GetWaypointByID(string ID)
		{
			if (!IDExists(ID)) return null;
			return waypoints[ID];
		}

		public static VicinityWaypoint CreateWaypoint(Vector3 position, float radius, IActor expectedActor)
		{
			//create a new vicinity waypoint
			VicinityWaypoint wp = Instantiate(m_instance.vicinityWaypointPrefab);
			//set position
			wp.Position = position;
			//set radius
			wp.Radius = radius;
			//set expected actor
			wp.ExpectedActor = expectedActor;
			//basic setup
			SetupBasicWaypoint(wp, PREFAB_TYPE_VICINITY_WAYPOINT);

			return wp;
		}

		public static AttachableWaypoint CreateAttachableWaypoint(IWaypointable waypointable, float radius, IActor expectedActor)
		{
			//create a attachable waypoint
			AttachableWaypoint wp = Instantiate(m_instance.attachableWaypointPrefab);
			//attach it to a waypointable object
			wp.AttachedWaypointable = waypointable;
			//set radius
			wp.Radius = radius;
			//set expected actor
			wp.ExpectedActor = expectedActor;
			//basic setup
			SetupBasicWaypoint(wp, PREFAB_TYPE_ATTACHABLE_WAYPOINT);

			return wp;
		}

		private static void SetupBasicWaypoint(IWaypoint wp, string prefabType)
		{
			//set prefab type
			wp.PrefabType = prefabType;
			//add waypoint to tracked list of waypoints
			AddWaypoint(wp);
			//add to holder transform
			if (wp is MonoBehaviour mono)
			{
				mono.transform.parent = WaypointParent;
			}
		}

		public const string PREFAB_TYPE_VAR_NAME = "Prefab Type";

		private const string TEMP_SAVE_FILE_NAME = "Waypoints_tmp",
			PERMANENT_SAVE_FILE_NAME = "Waypoints",
			SAVE_TAG_NAME = "Waypoints";
		
		public static void TemporarySave()
		{
			//delete existing temporary file
			DeleteTemporarySave();
			//reopen the file (which will recreate the file if second argument is true)
			UnifiedSaveLoad.OpenFile(TEMP_SAVE_FILE_NAME, true);
			//create a main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//loop over waypoints
			foreach (IWaypoint wp in waypoints.Values)
			{
				wp.Save(TEMP_SAVE_FILE_NAME, mainTag);
			}
			//save the data now in temp memory into a file
			UnifiedSaveLoad.SaveOpenedFile(TEMP_SAVE_FILE_NAME);
		}

		public static void PermanentSave()
		{
			//check if a temporary save file exists
			if (!SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME))
			{
				Debug.LogWarning("No temporary save file exists for the main hatch");
				return;
			}
			//copy data from temporary file
			string text = SaveLoad.LoadText(TEMP_SAVE_FILE_NAME);
			//overwrite the permanent file with the copied data
			SaveLoad.SaveText(PERMANENT_SAVE_FILE_NAME, text);
			//delete the temporary file
			DeleteTemporarySave();
		}

		public static void DeleteTemporarySave()
		{
			//delete existing temporary file
			SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
			//close the file because it was deleted
			UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
		}

		public static void Load()
		{
			Debug.Log("Waypoint Manager Data: Begin Loading");
			//check if save file exists
			bool tempSaveExists = SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME);
			bool permanentSaveExists = SaveLoad.RelativeSaveFileExists(PERMANENT_SAVE_FILE_NAME);
			string filename = null;
			if (tempSaveExists)
			{
				filename = TEMP_SAVE_FILE_NAME;
			}
			else if (permanentSaveExists)
			{
				filename = PERMANENT_SAVE_FILE_NAME;
			}
			else
			{
				Debug.Log("Waypoint Manager Data: Nothing to load, continuing");
				OnLoaded.Invoke();
				return;
			}

			//open the save file
			UnifiedSaveLoad.OpenFile(filename, false);
			//create save tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//iterate over main tag contents
			UnifiedSaveLoad.IterateTagContents(
				filename,
				mainTag,
				parameterCallBack: module => m_instance.ApplyData(module),
				subtagCallBack: subtag => m_instance.CheckSubtag(filename, subtag));
			Debug.Log("Waypoint Manager Data: Loaded");
			OnLoaded.Invoke();
		}

		private bool ApplyData(DataModule module)
		{
			return false;
		}

		private bool CheckSubtag(string filename, SaveTag subtag)
		{
			DataModule prefabTypeModule = UnifiedSaveLoad.GetModuleOfParameter(filename, subtag, PREFAB_TYPE_VAR_NAME);
			if (prefabTypeModule == DataModule.INVALID_DATA_MODULE) return false;

			IWaypoint wp = null;
			switch (prefabTypeModule.data)
			{
				default:
					return false;
				case PREFAB_TYPE_VICINITY_WAYPOINT:
				{
					wp = Instantiate(vicinityWaypointPrefab);
					break;
				}
				case PREFAB_TYPE_ATTACHABLE_WAYPOINT:
				{
					wp = Instantiate(attachableWaypointPrefab);
					break;
				}
			}

			if (wp != null)
			{
				UnifiedSaveLoad.IterateTagContents(
					filename,
					subtag,
					parameterCallBack: module => wp.ApplyData(module),
					subtagCallBack: st => wp.CheckSubtag(filename, st));

				SetupBasicWaypoint(wp, prefabTypeModule.data);
			}

			return true;
		}
	}
}
