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

		private const string SAVE_TAG_NAME = "Interaction Requirement";
		public override void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save waypoint ID
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, WaypointID);
		}
	}
}