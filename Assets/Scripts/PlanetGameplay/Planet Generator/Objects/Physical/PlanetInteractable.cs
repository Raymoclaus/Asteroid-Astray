using UnityEngine;

public abstract class PlanetInteractable : PlanetRoomObject
{
	[SerializeField] private InteractablePromptTrigger trigger;

	protected virtual void Awake() => trigger.OnInteraction += Interacted;

	protected virtual void Interacted(Triggerer actor)
	{
		if (actor is PlanetTriggerer && VerifyPlanetActor((PlanetTriggerer)actor))
		{
			((PlanetTriggerer)actor).Interacted(this);
		}
	}

	protected virtual bool VerifyPlanetActor(PlanetTriggerer actor) => true;

	public void EnableInteraction(bool enable) => trigger.EnableInteractionActions(enable);

	public void EnablePrompts(bool enable) => trigger.EnablePrompts(enable);

	public void EnableTrigger(bool enable) => trigger.EnableTrigger(enable);
}
