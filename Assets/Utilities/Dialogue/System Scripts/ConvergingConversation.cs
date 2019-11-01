using UnityEngine;

namespace DialogueSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Converging Conversation")]
	public class ConvergingConversation : ConversationEvent
	{
		public DialogueConvergeEvent convergeEvent;

		public override ConversationEventPosition GetNextConversation()
		{
			return new ConversationEventPosition(convergeEvent.nextConversation, convergeEvent.convergePoint);
		}
	}
}
