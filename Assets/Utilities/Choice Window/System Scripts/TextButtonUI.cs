using TMPro;
using UnityEngine;

public class TextButtonUI : ButtonUI
{
	[SerializeField] private TextMeshProUGUI label;

	public void SetLabelText(string text)
	{
		label.text = text;
	}
}
