namespace DialogueSystem
{
	public class ConversationEventPosition
	{
		public ConversationWithActions conversation;
		public int position;

		public ConversationEventPosition(ConversationWithActions conversation = null,
			int position = 0)
		{
			this.conversation = conversation;
			this.position = position;
		}
	}
}