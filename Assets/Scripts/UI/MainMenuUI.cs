using SaveSystem;
using SceneControllers;
using StatisticsTracker;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private MoveTriggerCanvasGroup mainMenu, loadingMenu;
	private List<MoveTriggerCanvasGroup> mtcg = new List<MoveTriggerCanvasGroup>();
	[SerializeField] private SceneChanger sceneChanger;
	[SerializeField] private StringStatTracker currentSceneTracker;

	private void Awake()
	{
		mtcg.Add(mainMenu);
		mtcg.Add(loadingMenu);

		MoveAll(mainMenu, true);
	}

	private void MoveAll(MoveTriggerCanvasGroup front, bool instant)
	{
		foreach (MoveTriggerCanvasGroup t in mtcg)
		{
			bool isFront = t == front;
			if (instant)
			{
				t.moveTrigger.InstantMove(isFront);
				t.canvasHider.InstantSetAlpha(isFront ? 1f : 0f);
			}
			else
			{
				t.moveTrigger.Move(t == front);
				t.canvasHider.SetTarget(isFront ? 1f : 0f);
			}
		}
	}

	public void StartButton()
	{
		//if a save file exists, go to loading menu window
		if (SaveReader.GetSaveFileCount() > 0)
		{
			OpenLoadingMenu();
		}
		//otherwise, instantly start a new game
		else
		{
			StartNewGame();
		}
	}

	public void StartNewGame()
	{
		StatisticsIO.ResetAllStats();
		string uniqueName = SaveLoad.GenerateUniqueSaveName();
		SaveLoad.CurrentSave = uniqueName;
		SaveReader.Save(uniqueName);
		
		sceneChanger.LoadScene(currentSceneTracker.Value);
	}

	public void OpenMainMenu()
	{
		MoveAll(mainMenu, false);
	}

	public void OpenLoadingMenu()
	{
		MoveAll(loadingMenu, false);
	}

	public void OpenSavesFolder()
	{
		string path = SaveLoad.path;
		Directory.CreateDirectory(path);
		if (!Directory.Exists(path)) return;
		Process.Start(path);
	}

	[System.Serializable]
	private class MoveTriggerCanvasGroup
	{
		public UIMoveTrigger moveTrigger;
		public UICanvasHider canvasHider;
	}
}
