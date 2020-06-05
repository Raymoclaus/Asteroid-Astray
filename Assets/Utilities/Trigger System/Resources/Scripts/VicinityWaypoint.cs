using System;
using TriggerSystem.Triggers;
using UnityEngine;

namespace TriggerSystem
{
	using QuestSystem;
	using SaveSystem;

	public class VicinityWaypoint : MonoBehaviour, IWaypoint
	{
		[SerializeField] private VicinityTrigger trigger;

		public IActor ExpectedActor { get; set; }
		public string ExpectedActorID => ExpectedActor?.UniqueID ?? string.Empty;

		public float Radius { get; set; }
		public string UniqueID { get; set; }
		public string PrefabType { get; set; }

		public event Action OnWaypointReached;

		private void Awake()
		{
			Subscribe();
		}

		private void OnDestroy()
		{
			Unsubscribe();
		}

		private void Subscribe()
		{
			if (TriggerIsNull) return;
			Unsubscribe();
			trigger.OnEnteredTrigger += EnteredTrigger;
		}

		private void Unsubscribe()
		{
			if (TriggerIsNull) return;
			trigger.OnEnteredTrigger -= EnteredTrigger;
		}

		private bool TriggerIsNull => trigger == null;

		private void EnteredTrigger(IActor actor)
		{
			if (actor.UniqueID != ExpectedActorID) return;
			OnWaypointReached?.Invoke();
		}

		public Vector3 Position
		{
			get => transform.position;
			set => transform.position = value;
		}

		public void Remove()
		{
			WaypointManager.RemoveWaypoint(this);

			Destroy(gameObject);
		}

		protected virtual string SaveTagName => $"{SAVE_TAG_NAME}:{UniqueID}";

		private const string SAVE_TAG_NAME = "Vicinity Waypoint",
			UNIQUE_ID_VAR_NAME = "Unique ID",
			POSITION_VAR_NAME = "Position",
			RADIUS_VAR_NAME = "Radius",
			EXPECTED_ACTOR_ID_VAR_NAME = "Expected Actor ID";

		public virtual void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save unique ID
			DataModule module = new DataModule(UNIQUE_ID_VAR_NAME, UniqueID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save position
			module = new DataModule(POSITION_VAR_NAME, Position);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save radius
			module = new DataModule(RADIUS_VAR_NAME, Radius);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save expected actor ID
			module = new DataModule(EXPECTED_ACTOR_ID_VAR_NAME, ExpectedActorID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save prefab type
			module = new DataModule(WaypointManager.PREFAB_TYPE_VAR_NAME, PrefabType);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		public virtual bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
				case UNIQUE_ID_VAR_NAME:
				{
					UniqueID = module.data;
					bool addedSuccessfully = UniqueIDGenerator.SetObjectToID(UniqueID, this);
					if (!addedSuccessfully)
					{
						Debug.Log("Could not be treated as a unique object.");
					}

					break;
				}
				case POSITION_VAR_NAME:
				{
					bool foundVal = module.data.TryParseToVector3(out Vector3 result);
					if (foundVal)
					{
						Position = result;
					}
					else
					{
						Debug.Log("Position data could not be parsed.");
					}
					
					break;
				}
				case RADIUS_VAR_NAME:
				{
					bool foundVal = float.TryParse(module.data, out float result);
					if (foundVal)
					{
						Radius = result;
					}
					else
					{
						Debug.Log("Radius data could not be parsed.");
					}

					break;
				}
				case EXPECTED_ACTOR_ID_VAR_NAME:
				{
					IUnique obj = UniqueIDGenerator.GetObjectByID(module.data);
					if (obj != null && obj is IActor actor)
					{
						ExpectedActor = actor;
					}
					else
					{
						UniqueIDGenerator.OnIDUpdated += WaitForObject;

						void WaitForObject(string ID)
						{
							if (ID != module.data) return;
							IUnique o = UniqueIDGenerator.GetObjectByID(ID);
							if (o != null && o is IActor a)
							{
								ExpectedActor = a;
								UniqueIDGenerator.OnIDUpdated -= WaitForObject;
							}
						}
					}

					break;
				}
			}

			return true;
		}

		public virtual bool CheckSubtag(string filename, SaveTag subtag)
		{
			return false;
		}
	}
}