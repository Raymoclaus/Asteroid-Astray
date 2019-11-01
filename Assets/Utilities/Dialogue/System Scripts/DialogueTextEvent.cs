using System;
using UnityEngine;

namespace DialogueSystem
{
	[Serializable]
	public class DialogueTextEvent : DialogueEvent
	{
		[SerializeField]
		[TextArea(1, 2)]
		public string line;
		public byte speakerID;
		public const string DEFAULT_LINE = "<No dialogue line available>";
	}
}
