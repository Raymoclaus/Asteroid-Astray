using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePromptTrigger : PromptTrigger
{
	[SerializeField] protected InputHandler.InputAction action = InputHandler.InputAction.Interact;

	protected virtual void Update()
	{
		if (IsTriggerActive() && InputHandler.GetInputDown(action) > 0f)
		{
			OnInteracted();
		}
	}

	protected virtual void OnInteracted()
	{

	}
}
