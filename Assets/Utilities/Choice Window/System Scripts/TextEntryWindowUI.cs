using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextEntryWindowUI : ChoiceWindowUI
{
	[SerializeField] private TMP_InputField inputField;

	public string Input => inputField.text;

	public void AddOnSubmitListener(UnityAction<string> listener)
	{
		inputField.onSubmit.AddListener(listener);
	}

	public void SelectInputField()
	{
		inputField.Select();
	}
}
