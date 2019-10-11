using System.Collections.Generic;
using UnityEngine;

public abstract class Triggerer : MonoBehaviour, IDirectionalActor, IInteractor
{
	private IPhysicsController physicsController;
	public IPhysicsController PhysicsController
		=> physicsController
		?? (physicsController = GetComponent<IPhysicsController>());

	public bool canTriggerPrompts = false;
	private List<ITrigger> nearbyTriggers = new List<ITrigger>();
	private HashSet<string> currentPerformingActions = new HashSet<string>();

	protected virtual void Update()
	{
		currentPerformingActions.Clear();
		for (int i = 0; i < nearbyTriggers.Count; i++)
		{
			if (nearbyTriggers[i] is IActionTrigger actionTrigger)
			{
				string action = actionTrigger.ActionRequired;
				if (currentPerformingActions.Contains(action)) continue;
				currentPerformingActions.Add(action);
				if (IsPerformingAction(action))
				{
					actionTrigger.Interact(this);
				}
			}
		}
	}

	public virtual void EnteredTrigger(ITrigger vTrigger)
	{
		nearbyTriggers.Add(vTrigger);
	}

	public virtual void ExitedTrigger(ITrigger vTrigger)
	{
		nearbyTriggers.Remove(vTrigger);
	}

	public Transform GetTransform => transform;

	private void RemoveNullTriggers()
	{
		for (int i = nearbyTriggers.Count - 1; i >= 0; i--)
		{
			if (nearbyTriggers[i] == null)
			{
				nearbyTriggers.RemoveAt(i);
			}
		}
	}

	private string GenerateKey(string text) => gameObject.GetInstanceID() + text;

	public abstract void Interact(object triggerObj);

	public virtual bool IsPerformingAction(string action) => false;

	public virtual object ObjectOrderRequest(object objectToGive) => null;

	public Vector3 FacingDirection => PhysicsController.FacingDirection;

	public bool CanTriggerPrompts => canTriggerPrompts;
}
