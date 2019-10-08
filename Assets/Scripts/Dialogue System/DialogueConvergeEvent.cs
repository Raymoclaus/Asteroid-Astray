using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Converge Event")]
public class DialogueConvergeEvent : ScriptableObject
{
	public ConversationWithActions nextConversation;
	public int convergePoint;

	public int GetConvergePoint()
	{
		return Mathf.Clamp(convergePoint, 0, nextConversation.Length - 1);
	}
}
