using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLineEvent
{
	[SerializeField]
	[TextArea(1, 2)]
	public string line;
	public byte speakerID;
	public const string DEFAULT_LINE = "<No dialogue line available>";
}
