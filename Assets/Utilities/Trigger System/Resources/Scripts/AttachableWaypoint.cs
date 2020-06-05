using QuestSystem;
using SaveSystem;
using UnityEngine;

namespace TriggerSystem
{
	public class AttachableWaypoint : VicinityWaypoint
	{
		public IWaypointable AttachedWaypointable { get; set; }
		public bool RemoveOnDetach { get; set; } = true;

		private void Update()
		{
			if (AttachedWaypointable == null || AttachedWaypointable.Equals(null))
			{
				Detach(RemoveOnDetach);
			}
			else
			{
				Position = AttachedWaypointable.Position;
			}
		}

		public void Detach(bool? remove = null)
		{
			if (remove != null)
			{
				RemoveOnDetach = (bool)remove;
			}

			AttachedWaypointable = null;

			if (RemoveOnDetach)
			{
				Remove();
			}
		}

		protected override string SaveTagName => $"{SAVE_TAG_NAME}:{UniqueID}";

		private const string SAVE_TAG_NAME = "Attachable Waypoint",
			ATTACHED_WAYPOINTABLE_ID_VAR_NAME = "Attached Waypointable ID",
			REMOVE_ON_DETACH_VAR_NAME = "Remove On Detach";

		public override void Save(string filename, SaveTag parentTag)
		{
			base.Save(filename, parentTag);
			
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save attached waypointable ID
			DataModule module = new DataModule(ATTACHED_WAYPOINTABLE_ID_VAR_NAME, AttachedWaypointable.UniqueID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save remove on detach boolean
			module = new DataModule(REMOVE_ON_DETACH_VAR_NAME, RemoveOnDetach);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		public override bool ApplyData(DataModule module)
		{
			if (base.ApplyData(module)) return true;

			switch (module.parameterName)
			{
				default:
					return false;
				case ATTACHED_WAYPOINTABLE_ID_VAR_NAME:
				{
					IUnique obj = UniqueIDGenerator.GetObjectByID(module.data);
					if (obj != null && obj is IWaypointable wp)
					{
						AttachedWaypointable = wp;
					}
					else
					{
						UniqueIDGenerator.OnIDUpdated += WaitForObject;
						enabled = false;

						void WaitForObject(string ID)
						{
							if (ID != module.data) return;
							IUnique o = UniqueIDGenerator.GetObjectByID(ID);
							if (o != null && o is IWaypointable w)
							{
								AttachedWaypointable = w;
								UniqueIDGenerator.OnIDUpdated -= WaitForObject;
								enabled = true;
							}
						}
					}

					break;
				}
				case REMOVE_ON_DETACH_VAR_NAME:
				{
					bool foundVal = bool.TryParse(module.data, out bool result);
					if (foundVal)
					{
						RemoveOnDetach = result;
					}
					else
					{
						Debug.Log("Remove On Detach data could not be parsed.");
					}

					break;
				}
			}

			return true;
		}

		public override bool CheckSubtag(string filename, SaveTag subtag)
		{
			if (base.CheckSubtag(filename, subtag)) return true;

			return false;
		}
	} 
}
