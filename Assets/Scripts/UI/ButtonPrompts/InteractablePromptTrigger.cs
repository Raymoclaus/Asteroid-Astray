using UnityEngine;
using UnityEngine.Events;

public class InteractablePromptTrigger : PromptTrigger
{
	[SerializeField] protected InputAction action = InputAction.Interact;

	public delegate void InteractionEventHandler();
	public event InteractionEventHandler OnInteraction;

	private static DialogueController dialogueController;
	protected static DialogueController DialogueController
	{
		get
		{
			return dialogueController ?? (dialogueController = FindObjectOfType<DialogueController>());
		}
	}
	protected bool enabledInteractionActions = true;

	[SerializeField] protected UnityEvent interactionActions;

	protected override void Awake()
	{
		base.Awake();
		OnInteraction += OnInteracted;
	}

	protected virtual void Update()
	{
		if (IsTriggerActive() && InputHandler.GetInputDown(action) > 0f && !Pause.IsStopped)
		{
			OnInteraction?.Invoke();
		}
	}

	protected virtual void OnInteracted()
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
