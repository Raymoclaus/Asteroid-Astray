using DialogueSystem;
using UnityEngine;

[RequireComponent(typeof(IChatter))]
public class ChatBehaviour : MonoBehaviour
{
	private IChatter chatter;
	private IChatter Chatter => chatter != null
		? chatter
		: (chatter = GetComponent<IChatter>());

	[SerializeField] private DialogueController activeDialoguePrefab,
		passiveDialoguePrefab;
	private DialogueController activeDialogue, passiveDialogue;

	private void Awake()
	{
		Chatter.OnSendActiveDialogue += SendActiveDialogue;
		Chatter.OnSendPassiveDialogue += SendPassiveDialogue;
	}

	private void OnDestroy()
	{
		Chatter.OnSendActiveDialogue -= SendActiveDialogue;
		Chatter.OnSendPassiveDialogue -= SendPassiveDialogue;
	}

	private void SendActiveDialogue(ConversationWithActions dialogue, bool skip)
	{
		if (!Chatter.CanSendDialogue) return;
		ActiveDialogue?.StartDialogue(dialogue, skip);
	}

	private void SendPassiveDialogue(ConversationWithActions dialogue, bool skip)
	{
		if (!Chatter.CanSendDialogue) return;
		PassiveDialogue?.StartDialogue(dialogue, skip);
	}

	protected DialogueController ActiveDialogue
	{
		get
		{
			if (activeDialogue != null) return activeDialogue;
			activeDialogue = FindObjectOfType<ActiveDialogueController>();
			if (activeDialogue != null) return activeDialogue;
			if (activeDialoguePrefab == null) return null;
			return activeDialogue = Instantiate(activeDialoguePrefab);
		}
	}

	protected DialogueController PassiveDialogue
	{
		get
		{
			if (passiveDialogue != null) return passiveDialogue;
			passiveDialogue = FindObjectOfType<PassiveDialogueController>();
			if (passiveDialogue != null) return passiveDialogue;
			if (passiveDialoguePrefab == null) return null;
			return passiveDialogue = Instantiate(passiveDialoguePrefab);
		}
	}
}
