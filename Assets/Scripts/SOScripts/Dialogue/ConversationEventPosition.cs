public class ConversationEventPosition
{
	public ConversationEvent conversation;
	public int position;

	public ConversationEventPosition(ConversationEvent conversation = null, int position = 0)
	{
		this.conversation = conversation;
		this.position = position;
	}
}