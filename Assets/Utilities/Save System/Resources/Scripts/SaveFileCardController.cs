using SaveSystem;
using TMPro;
using UnityEngine;
using System.IO;
using SceneControllers;
using StatisticsTracker;

public class SaveFileCardController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI fileNameText;
	[SerializeField] private string fileNamePrefix = "File: ";
	private SaveFile file;
	private SaveFileCardGenerator generator;
	[SerializeField] private SceneChanger sceneChanger;

	public string FileName => file.Name;

	public void SetFileName(string name)
	{
		fileNameText.text = $"{fileNamePrefix}{name}";
	}

	public void AttachSaveFile(SaveFile file)
	{
		this.file = file;
		SetFileName(FileName);
	}

	public void SetGenerator(SaveFileCardGenerator generator)
	{
		this.generator = generator;
	}

	public void EditNameButton()
	{
		generator.RenameFile(this);
	}

	public bool EditName(string newName)
	{
		string newPath = $"{file.dirInfo.Parent.FullName}/{newName}";
		try
		{
			Directory.Move(file.dirInfo.FullName, newPath);
			SetFileName(newName);
			file.dirInfo = new DirectoryInfo(newPath);
			return true;
		}
		catch (DirectoryNotFoundException e)
		{
			Debug.Log(e);
			return false;
		}
		catch (IOException e)
		{
			Debug.Log(e);
			return false;
		}
	}

	public void DeleteButton()
	{
		generator.DeleteFile(this);
	}

	public void DeleteFile()
	{
		file.dirInfo.Delete(true);
		Destroy(gameObject);
	}

	public void CopyFile()
	{
		generator.CreateCopy(file);
	}

	public void LoadFileButton()
	{
		SaveLoad.CurrentSave = FileName;
		StatisticsIO.Load();

		StatTracker currentScene = StatisticsIO.GetTracker("Current Scene");
		SceneTracker.AttachToSceneLoader();
		sceneChanger.LoadScene(currentScene.ValueString);
	}
}
