using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Line")]
public class DialogueLineEvent : ScriptableObject
{
	[SerializeField]
	protected string[] textVariations;
	public byte speakerID;
	public const string DEFAULT_LINE = "<No dialogue line available>";

	public virtual string GetLine()
	{
		if (textVariations.Length == 0) return DEFAULT_LINE;
		if (textVariations.Length == 1) return textVariations[0];

		int choose = Random.Range(0, textVariations.Length);
		return textVariations[choose];
	}
}
