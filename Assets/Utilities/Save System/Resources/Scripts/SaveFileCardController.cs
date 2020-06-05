using InventorySystem;
using QuestSystem;
using SaveSystem;
using SceneControllers;
using StatisticsTracker;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveFileCardController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI fileNameText;
	[SerializeField] private string fileNamePrefix = "File: ";
	private SaveFile file;
	private SaveFileCardGenerator generator;
	[SerializeField] private SceneChanger sceneChanger;
	[SerializeField] private StringStatTracker currentSceneTracker;

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

		EntityNetwork.DeleteTemporarySave();
		MainHatchPrompt.DeleteTemporarySave();
		NarrativeManager.DeleteTemporarySave();
		WaypointManager.DeleteTemporarySave();
		StatisticsIO.ResetAllStats();
		ScriptedDropsIO.Reset();

		StatisticsIO.Load();
		ScriptedDropsIO.Load();
		
		sceneChanger.LoadScene(currentSceneTracker.ValueString);
	}
}
