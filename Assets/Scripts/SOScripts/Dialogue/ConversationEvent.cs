using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Basic Conversation")]
public class ConversationEvent : ScriptableObject
{
	[SerializeField]
	public DialogueLineEvent[] conversation;
	public EntityProfile[] speakers;

	public virtual ConversationEventPosition GetNextConversation()
	{
		return null;
	}

	public virtual string[] GetLines()
	{
		if (conversation.Length == 0) return new string[] { DialogueLineEvent.DEFAULT_LINE };

		string[] lines = new string[conversation.Length];
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i] = conversation[i].GetLine();
		}
		return lines;
	}

	public void HasAction(int i, bool b)
	{
		conversation[i].SetHasAction(b);
	}

	public bool GetHasAction(int i)
	{
		return conversation[i].hasAction;
	}
}