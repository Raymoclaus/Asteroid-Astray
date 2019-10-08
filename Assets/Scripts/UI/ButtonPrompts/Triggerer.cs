using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public abstract class Triggerer : MonoBehaviour
{
	private PromptUI prompts;
	private PromptUI Prompts => prompts ?? (prompts = FindObjectOfType<PromptUI>());
	public bool canTriggerPrompts = false;
	private List<VicinityTrigger> nearbyTriggers = new List<VicinityTrigger>();
	private PromptTrigger currentPromptTrigger;
	private HashSet<string> currentPerformingActions = new HashSet<string>();

	protected virtual void Update()
	{
		currentPerformingActions.Clear();
		for (int i = 0; i < nearbyTriggers.Count; i++)
		{
			VicinityTrigger trig = nearbyTriggers[i];
			if (nearbyTriggers[i] is InteractablePromptTrigger)
			{
				InteractablePromptTrigger ipt = (InteractablePromptTrigger)trig;
				if (!ipt.CanBeInteractedWith) continue;
				string action = ipt.Action;
				if (currentPerformingActions.Contains(action)) continue;
				currentPerformingActions.Add(action);
				if (IsPerformingAction(action))
				{
					ipt.Interact(this);
				}
			}
		}
	}

	public virtual void EnteredTrigger(VicinityTrigger vTrigger)
	{
		nearbyTriggers.Add(vTrigger);
		if (currentPromptTrigger == null)
		{
			GetNextPromptTrigger();
		}
	}

	public virtual void ExitedTrigger(VicinityTrigger vTrigger)
	{
		nearbyTriggers.Remove(vTrigger);
		if (vTrigger == currentPromptTrigger)
		{
			GetNextPromptTrigger();
		}
	}

	private void GetNextPromptTrigger()
	{
		nearbyTriggers.RemoveAll(t => t == null);
		PromptTrigger nextPromptTrigger = (PromptTrigger)nearbyTriggers.FirstOrDefault(
						t => (t is PromptTrigger) && ((PromptTrigger)t).PromptsEnabled);
		if (nextPromptTrigger == null && currentPromptTrigger != null)
		{
			Prompts.DeactivatePrompt(currentPromptTrigger.text);
		}
		currentPromptTrigger = nextPromptTrigger;
		if (currentPromptTrigger == null) return;
		Prompts.ActivatePrompt(currentPromptTrigger.text);
	}

	protected virtual bool IsPerformingAction(string action) => false;

	public virtual bool RequestObject(object objectToGive) => false;
}
