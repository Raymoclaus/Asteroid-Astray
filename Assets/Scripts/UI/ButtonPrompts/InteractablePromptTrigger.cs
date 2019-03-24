using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePromptTrigger : PromptTrigger
{
	[SerializeField] protected InputAction action = InputAction.Interact;

	public delegate void InteractionEventHandler();
	public event InteractionEventHandler OnInteraction;
	public void Interaction() => OnInteraction?.Invoke();

	private void Awake()
	{
		OnInteraction += OnInteracted;
	}

	protected virtual void Update()
	{
		if (IsTriggerActive() && InputHandler.GetInputDown(action) > 0f)
		{
			Interaction();
		}
	}

	protected virtual void OnInteracted()
	{

	}
}
