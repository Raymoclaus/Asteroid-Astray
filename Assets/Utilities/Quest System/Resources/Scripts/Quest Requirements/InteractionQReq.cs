using SaveSystem;
using TriggerSystem;
using UnityEngine;

namespace QuestSystem.Requirements
{
	public class InteractionQReq : QuestRequirement
	{
		private IInteractable interactable;
		private string InteractableID { get; set; }
		private IInteractor expectedInteractor;
		private string ExpectedInteractorID { get; set; }

		protected InteractionQReq() : base()
		{

		}

		public InteractionQReq(IInteractable interactable,
			IInteractor expectedInteractor, string description, IWaypoint waypoint)
			: base(description, waypoint)
		{
			this.interactable = interactable;
			InteractableID = interactable.UniqueID;
			this.expectedInteractor = expectedInteractor;
			ExpectedInteractorID = expectedInteractor.UniqueID;
		}

		public override void Activate()
		{
			base.Activate();
			interactable.OnInteracted += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			interactable.OnInteracted -= EvaluateEvent;
		}

		private void EvaluateEvent(IInteractor interactor)
		{
			if (Completed)
			{
				Debug.Log("Quest requirement already completed.");
				return;
			}

			if (expectedInteractor != interactor) return;

			QuestRequirementCompleted();
		}

		private const string INTERACTABLE_ID_VAR_NAME = "Interactable ID",
			INTERACTOR_ID_VAR_NAME = "Interactor ID";

		public override void Save(string filename, SaveTag parentTag)
		{
			base.Save(filename, parentTag);

			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save interactable ID
			DataModule module = new DataModule(INTERACTABLE_ID_VAR_NAME, InteractableID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save interactor ID
			module = new DataModule(INTERACTOR_ID_VAR_NAME, ExpectedInteractorID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}

		protected override bool ApplyData(DataModule module)
		{
			if (base.ApplyData(module)) return true;

			switch (module.parameterName)
			{
				default:
					return false;
				case INTERACTABLE_ID_VAR_NAME:
					InteractableID = module.data;
					if (InteractableID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(InteractableID);
						if (obj is IInteractable ia)
						{
							interactable = ia;
						}
					}
					break;
				case INTERACTOR_ID_VAR_NAME:
					ExpectedInteractorID = module.data;
					if (ExpectedInteractorID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(ExpectedInteractorID);
						if (obj is IInteractor ia)
						{
							expectedInteractor = ia;
						}
					}
					break;
			}

			return true;
		}
	}
}