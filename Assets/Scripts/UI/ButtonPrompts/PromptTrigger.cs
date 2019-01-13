﻿using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
	[SerializeField]
	private PromptUI promptUI;
	private int promptLayer;
	private List<IPromptRespone> prompts = new List<IPromptRespone>();

	private void Awake()
	{
		promptLayer = LayerMask.NameToLayer("ButtonPrompt");
		if (!promptUI)
		{
			foreach (Canvas canvas in FindObjectsOfType<Canvas>())
			{
				promptUI = canvas.GetComponentInChildren<PromptUI>(true);
				if (promptUI) break;
			}
		}
	}

	private void Update()
	{
		promptUI = promptUI ?? FindObjectOfType<PromptUI>();
		if (!promptUI) return;

		promptUI.gameObject.SetActive(prompts.Count > 0);

		//check next prompt in list
		if (Input.GetKeyDown(KeyCode.Q) && prompts.Count > 1)
		{
			IPromptRespone pr = prompts[0];
			prompts.RemoveAt(0);
			prompts.Add(pr);
		}

		if (prompts.Count > 0)
		{
			promptUI.textUI.text = prompts[0].InteractString();

			if (prompts[0].CheckResponse())
			{
				prompts[0].Execute();
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer != promptLayer) return;

		IPromptRespone pr = collision.GetComponent<IPromptRespone>();
		pr.Enter();
		prompts.Add(collision.GetComponent<IPromptRespone>());
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.layer != promptLayer) return;

		IPromptRespone pr = collision.GetComponent<IPromptRespone>();
		pr.Exit();
		prompts.Remove(pr);
	}
}