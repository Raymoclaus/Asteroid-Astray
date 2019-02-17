using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLineEvent
{
	[SerializeField]
	[TextArea(1, 2)]
	public string line;
	public byte speakerID;
	[HideInInspector] public bool hasAction;
	[HideInInspector] public UnityEvent action, skipAction;
	public const string DEFAULT_LINE = "<No dialogue line available>";

	public void SetHasAction(bool b)
	{
		hasAction = b;
	}
}
