using System.Collections.Generic;
using System.IO;
using SaveSystem;
using UnityEngine;
using UnityEngine.Events;

public class SaveFileCardGenerator : MonoBehaviour
{
	private const string DELETE_FILE_MESSAGE = "Are you sure you want to permanently delete file \"{0}\"?",
		RENAME_FILE_MESSAGE = "Choose a new name for file \"{0}\".";
	[SerializeField] private SaveFileCardController placecardPrefab;
	[SerializeField] private ChoiceWindowGenerator choiceWindowGenerator;
	[SerializeField] private Sprite tickIcon, crossIcon;

	private void Awake()
	{
		//remove existing children
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}
		//get list of save files
		List<SaveFile> saves = SaveReader.GetSaves();
		//read save files and create placecards for each one
		foreach (SaveFile save in saves)
		{
			CreateCard(save);
		}
	}

	private void CreateCard(SaveFile file)
	{
		SaveFileCardController sfcc = Instantiate(placecardPrefab, transform);
		sfcc.AttachSaveFile(file);
		sfcc.SetGenerator(this);
	}

	public void CreateCopy(SaveFile file)
	{
		DirectoryInfo original = file.dirInfo;
		DirectoryInfo copy = original.CopyToLocation(original.Parent);
		if (copy == null) return;
		SaveFile newFile = new SaveFile(copy);
		CreateCard(newFile);
	}

	public void DeleteFile(SaveFileCardController sfcc)
	{
		ChoiceWindowUI window = choiceWindowGenerator.CreateChoiceWindow();
		string message = string.Format(DELETE_FILE_MESSAGE, sfcc.FileName);
		window.SetMessage(message);
		UnityAction cancelListener = () =>
		{
			Destroy(window.gameObject);
		};
		UnityAction deleteListener = () =>
		{
			sfcc.DeleteFile();
			cancelListener?.Invoke();
		};
		window.AddIconButton(tickIcon, deleteListener);
		IconButtonUI cancelButton = window.AddIconButton(crossIcon, cancelListener);
		cancelButton.Select();
	}

	public void RenameFile(SaveFileCardController sfcc)
	{
		TextEntryWindowUI window = choiceWindowGenerator.CreateTextEntryWindow();
		string message = string.Format(RENAME_FILE_MESSAGE, sfcc.FileName);
		window.SetMessage(message);
		UnityAction cancelListener = () =>
		{
			Destroy(window.gameObject);
		};
		UnityAction confirmListener = () =>
		{
			string input = window.Input;
			if (!string.IsNullOrEmpty(input))
			{
				sfcc.EditName(input);
			}

			cancelListener?.Invoke();
		};
		UnityAction<string> submitListener = s =>
		{
			if (!string.IsNullOrEmpty(s))
			{
				sfcc.EditName(s);
			}

			cancelListener?.Invoke();
		};
		window.AddIconButton(tickIcon, confirmListener);
		window.AddIconButton(crossIcon, cancelListener);
		window.AddOnSubmitListener(submitListener);
		window.SelectInputField();
	}
}
