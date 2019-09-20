using UnityEngine;
using UnityEngine.Events;

public class InteractablePromptTrigger : PromptTrigger
{
	public delegate void InteractionEventHandler(Triggerer actor);
	public event InteractionEventHandler OnInteraction;

	private static DialogueController dlgCtrl;
	protected static DialogueController DlgCtrl
		=> dlgCtrl ?? (dlgCtrl = FindObjectOfType<DialogueController>());
	protected bool enabledInteractionActions = true;
	[SerializeField] protected string action = "Interact";
	public string Action => action;

	[SerializeField] protected UnityEvent interactionActions;

	protected override void Awake()
	{
		base.Awake();
		OnInteraction += OnInteracted;
	}

	protected virtual void Update()
	{
		for (int i = 0; i < nearbyActors.Count; i++)
		{
			if (!Pause.IsStopped && nearbyActors[i].IsInteracting(this))
			{
				OnInteraction?.Invoke(nearbyActors[i]);
			}
		}
	}

	protected virtual void OnInteracted(Triggerer actor)
	{
		if (enabledInteractionActions)
		{
			ActivateInteractionActions();
		}
	}

	protected virtual void ActivateInteractionActions() => interactionActions?.Invoke();

	public void AddInteractionAction(UnityAction action)
		=> interactionActions.AddListener(action);

	public void RemoveInteractionAction(UnityAction action)
		=> interactionActions.RemoveListener(action);

	public void RemoveAllInteractionActions() => interactionActions.RemoveAllListeners();

	public void EnableInteractionActions(bool enable) => enabledInteractionActions = enable;
}
