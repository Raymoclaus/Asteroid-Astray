using UnityEngine;

public class ChoiceWindowGenerator : MonoBehaviour
{
	[SerializeField] private ChoiceWindowUI choiceWindowPrefab;
	[SerializeField] private TextEntryWindowUI textEntryWindowPrefab;

	public ChoiceWindowUI CreateChoiceWindow()
	{
		ChoiceWindowUI newWindow = Instantiate(choiceWindowPrefab, transform);
		return newWindow;
	}

	public TextEntryWindowUI CreateTextEntryWindow()
	{
		TextEntryWindowUI newWindow = Instantiate(textEntryWindowPrefab, transform);
		return newWindow;
	}
}
