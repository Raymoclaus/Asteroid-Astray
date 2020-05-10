using SaveSystem;
using TriggerSystem;

namespace QuestSystem.Requirements
{
	public class InteractionQReq : QuestRequirement
	{
		private IInteractable interactable;
		private string InteractableID { get; set; }
		private IInteractor expectedInteractor;
		private string ExpectedInteractorID { get; set; }

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
			if (Completed || !active) return;

			if (expectedInteractor != interactor) return;

			QuestRequirementCompleted();
		}

		private const string REQUIREMENT_TYPE = "Interaction Requirement",
			INTERACTABLE_ID_VAR_NAME = "Interactable ID",
			INTERACTOR_ID_VAR_NAME = "Interactor ID";

		public override string GetRequirementType() => REQUIREMENT_TYPE;

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
	}
}