using UnityEngine;

namespace DialogueSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Branching Conversation")]
	public class BranchingConversation : ConversationEvent
	{
		public BranchEvent branch;

		public override ConversationEventPosition GetNextConversation()
		{
			return new ConversationEventPosition(branch.GetConversation(), 0);
		}
	}
}
