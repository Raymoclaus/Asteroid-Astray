using System;

namespace DialogueSystem
{
	[Serializable]
	public class DialogueWaitEvent : DialogueEvent
	{
		public float waitDuration = 3f;
	}
}
