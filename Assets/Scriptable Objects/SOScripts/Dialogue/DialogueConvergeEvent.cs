using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Converge Event")]
public class DialogueConvergeEvent : ScriptableObject
{
	public ConversationEvent nextConversation;
	public int convergePoint;

	public int GetConvergePoint()
	{
		return Mathf.Clamp(convergePoint, 0, nextConversation.conversation.Length - 1);
	}
}
