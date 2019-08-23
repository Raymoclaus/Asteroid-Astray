using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Triggerer : MonoBehaviour
{
	public bool canTriggerPrompts = false;

	public virtual void EnteredTrigger(VicinityTrigger vTrigger)
	{

	}

	public virtual void ExitedTrigger(VicinityTrigger vTrigger)
	{

	}

	public virtual bool IsInteracting(InteractablePromptTrigger trigger) => false;

	public virtual void Interacted(InteractablePromptTrigger trigger) { }
}
