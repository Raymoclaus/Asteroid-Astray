using InputHandlerSystem.UI;
using UnityEngine;

public class PassivePrompt : MonoBehaviour
{
	[SerializeField] private InputIconTextMesh textMesh;

	public void SetText(string text) => textMesh.SetText(text);

	public void SetActive(bool activate)
	{
		gameObject.SetActive(activate);
	}
}
