using UnityEngine;
using UnityEngine.UI;

public class PromptUI : MonoBehaviour
{
	public Text textUI;

	private void Awake()
	{
		textUI = textUI ?? GetComponentInChildren<Text>();
	}
}
