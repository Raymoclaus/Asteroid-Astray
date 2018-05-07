using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPrompt : MonoBehaviour
{
	public GameObject promptUI;
	public Text promptText;
	private int promptLayer;
	private List<IPromptRespone> prompts = new List<IPromptRespone>();

	private void Awake()
	{
		promptLayer = LayerMask.NameToLayer("ButtonPrompt");
	}

	private void Update()
	{
		promptUI.SetActive(prompts.Count > 0);

		//check next prompt in list
		if (Input.GetKeyDown(KeyCode.Q) && prompts.Count > 1)
		{
			IPromptRespone pr = prompts[0];
			prompts.RemoveAt(0);
			prompts.Add(pr);
		}

		if (prompts.Count > 0)
		{
			promptText.text = prompts[0].InteractString();

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