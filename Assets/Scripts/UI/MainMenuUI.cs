﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SaveSystem;
using SceneControllers;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private MoveTriggerCanvasGroup mainMenu, loadingMenu;
	private List<MoveTriggerCanvasGroup> mtcg = new List<MoveTriggerCanvasGroup>();
	[SerializeField] private SceneChanger sceneChanger;

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
		sceneChanger.LoadScene("WormholeScene");
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
		Directory.CreateDirectory(SaveLoad.path);
		if (!Directory.Exists(SaveLoad.path)) return;
		Process.Start(SaveLoad.path);
	}

	[System.Serializable]
	private class MoveTriggerCanvasGroup
	{
		public UIMoveTrigger moveTrigger;
		public UICanvasHider canvasHider;
	}
}
