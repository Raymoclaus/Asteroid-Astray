using UnityEngine;

[RequireComponent(typeof(InteractablePromptTrigger))]
public abstract class PlanetInteractable : PlanetRoomObject
{
	private InteractablePromptTrigger trigger;
	private InteractablePromptTrigger Trigger
		=> trigger ?? (trigger = GetComponent<InteractablePromptTrigger>());

	private void Awake() => Trigger.OnInteraction += Interacted;

	protected virtual void Interacted(Triggerer actor)
	{
		if (actor is PlanetTriggerer && VerifyPlanetActor((PlanetTriggerer)actor))
		{
			((PlanetTriggerer)actor).Interacted(this);
		}
	}

	protected virtual bool VerifyPlanetActor(PlanetTriggerer actor) => true;

	public void EnableInteraction(bool enable) => Trigger.EnableInteractionActions(enable);

	public void EnablePrompts(bool enable) => Trigger.EnablePrompts(enable);

	public void EnableTrigger(bool enable) => Trigger.EnableTrigger(enable);
}
