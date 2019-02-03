using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Line")]
public class DialogueLineEvent : ScriptableObject
{
	[SerializeField]
	[TextArea(1, 2)]
	protected string[] textVariations;
	public byte speakerID;
	//[HideInInspector]
	public bool hasAction;
	//[HideInInspector]
	public UnityEvent action;
	public const string DEFAULT_LINE = "<No dialogue line available>";

	public virtual string GetLine()
	{
		if (textVariations.Length == 0) return DEFAULT_LINE;
		if (textVariations.Length == 1) return textVariations[0];

		int choose = Random.Range(0, textVariations.Length);
		return textVariations[choose];
	}

	public virtual string GetLine(int specificVariation)
	{
		int clamped = Mathf.Clamp(specificVariation, 0, textVariations.Length);
		return textVariations[clamped];
	}

	public void SetHasAction(bool b)
	{
		hasAction = b;
	}
}
