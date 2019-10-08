using UnityEngine;
using UnityEngine.Events;

public class InteractablePromptTrigger : PromptTrigger
{
	public delegate void InteractionEventHandler(Triggerer actor);
	public event InteractionEventHandler OnInteraction;
	
	protected bool enabledInteractionActions = true;
	[SerializeField] protected string action = "Interact";
	public string Action => action;

	public virtual bool CanBeInteractedWith
	{
		get => enabledInteractionActions;
		set => enabledInteractionActions = value;
	}

	public virtual void Interact(Triggerer actor)
		=> OnInteraction?.Invoke(actor);
}
