using SaveSystem;
using System;
using UnityEngine;

namespace QuestSystem
{
	public abstract class QuestRequirement
	{
		protected string description;
		public IWaypoint Waypoint { get; private set; }
		public string WaypointID { get; private set; } = string.Empty;
		public int id;
		public bool Completed { get; private set; }
		public bool RemoveWaypointOnCompletion { get; set; } = true;

		public event Action OnQuestRequirementUpdated, OnQuestRequirementCompleted;

		protected QuestRequirement()
		{

		}

		public QuestRequirement(string description, IWaypoint waypoint)
		{
			this.description = description;
			Waypoint = waypoint;
			WaypointID = waypoint?.UniqueID ?? string.Empty;
		}

		public virtual void Activate()
		{

		}
		
		public void QuestRequirementUpdated()
		{
			SteamPunkConsole.WriteLine($"Requirement Updated: {GetDescription}");
			OnQuestRequirementUpdated?.Invoke();
		}
		
		public virtual void QuestRequirementCompleted()
		{
			Completed = true;
			if (Waypoint != null && !Waypoint.Equals(null))
			{
				Waypoint.Remove();
			}
			OnQuestRequirementCompleted?.Invoke();
		}

		public virtual string GetDescription => description;

		public string SaveTagName => $"{SAVE_TAG_NAME}:{id}";

		private const string SAVE_TAG_NAME = "Quest Requirement",
			DESCRIPTION_VAR_NAME = "Description",
			WAYPOINTID_VAR_NAME = "Waypoint ID",
			COMPLETED_VAR_NAME = "Completed",
			ACTIVE_VAR_NAME = "Active",
			REQUIREMENT_TYPE_VAR_NAME = "Requirement Type",
			REQUIREMENT_ID_VAR_NAME = "Requirement ID",
			WAYPOINT_REMOVAL_VAR_NAME = "Remove Waypoint On Completion";

		public virtual void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save description
			DataModule module = new DataModule(DESCRIPTION_VAR_NAME, description);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save requirement type
			module = new DataModule(REQUIREMENT_TYPE_VAR_NAME, GetType().AssemblyQualifiedName);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save waypoint ID
			module = new DataModule(WAYPOINTID_VAR_NAME, WaypointID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save completed state
			module = new DataModule(COMPLETED_VAR_NAME, Completed);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save requirement ID
			module = new DataModule(REQUIREMENT_ID_VAR_NAME, id);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save waypoint removal option
			module = new DataModule(WAYPOINT_REMOVAL_VAR_NAME, RemoveWaypointOnCompletion);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		public static bool RecogniseTag(SaveTag tag)
		{
			return tag.TagName.StartsWith(SAVE_TAG_NAME);
		}

		public static QuestRequirement LoadQuestRequirementFromFile(string filename, SaveTag tag)
		{
			DataModule typeModule = UnifiedSaveLoad.GetModuleOfParameter(filename, tag, REQUIREMENT_TYPE_VAR_NAME);
			Type requirementType = Type.GetType(typeModule.data);
			Type baseRequirementType = typeof(QuestRequirement);
			if (requirementType == null
			    || requirementType == baseRequirementType
			    || !requirementType.IsSubclassOf(baseRequirementType))
			{
				Debug.Log("Requirement type data could not be parsed.");
				return null;
			}

			QuestRequirement qr = (QuestRequirement) Activator.CreateInstance(requirementType, true);

			UnifiedSaveLoad.IterateTagContents(
				filename,
				tag,
				parameterCallBack: module => qr.ApplyData(module),
				subtagCallBack: subtag => qr.CheckSubtag(filename, subtag));
			
			return qr;
		}

		protected virtual bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
				case DESCRIPTION_VAR_NAME:
				{
					description = module.data;
					break;
				}
				case WAYPOINTID_VAR_NAME:
				{
					WaypointID = module.data;
					if (WaypointID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(WaypointID);
						if (obj != null && obj is IWaypoint wp)
						{
							Waypoint = wp;
						}
						else
						{
							UniqueIDGenerator.OnIDUpdated += WaitForObject;

							void WaitForObject(string ID)
							{
								if (ID != WaypointID) return;
								IUnique o = UniqueIDGenerator.GetObjectByID(ID);
								if (o != null && o is IWaypoint w)
								{
									Waypoint = w;
									UniqueIDGenerator.OnIDUpdated -= WaitForObject;
								}
							}
						}
					}

					break;
				}
				case COMPLETED_VAR_NAME:
				{
					bool foundVal = bool.TryParse(module.data, out bool val);
					if (foundVal)
					{
						Completed = val;
					}
					else
					{
						Debug.Log("Completed data could not be parsed.");
					}

					break;
				}
				case REQUIREMENT_ID_VAR_NAME:
				{
					bool foundVal = int.TryParse(module.data, out int val);
					if (foundVal)
					{
						id = val;
					}
					else
					{
						Debug.Log("Requirement ID data could not be parsed.");
					}

					break;
				}
			}

			return true;
		}

		protected virtual bool CheckSubtag(string filename, SaveTag subtag)
		{
			return false;
		}
	}
}