using UnityEngine;

public class ChoiceWindowGenerator : MonoBehaviour
{
	private static ChoiceWindowGenerator _instance;

	[SerializeField] private ChoiceWindowUI choiceWindowPrefab;
	[SerializeField] private TextEntryWindowUI textEntryWindowPrefab;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public static ChoiceWindowUI CreateChoiceWindow()
	{
		ChoiceWindowUI newWindow = Instantiate(_instance.choiceWindowPrefab, _instance.transform);
		return newWindow;
	}

	public static TextEntryWindowUI CreateTextEntryWindow()
	{
		TextEntryWindowUI newWindow = Instantiate(_instance.textEntryWindowPrefab, _instance.transform);
		return newWindow;
	}
}
