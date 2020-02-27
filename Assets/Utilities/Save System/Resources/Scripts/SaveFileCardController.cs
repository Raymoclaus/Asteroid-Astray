using TMPro;
using UnityEngine;

public class SaveFileCardController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI fileNameText;
	[SerializeField] private string fileNamePrefix = "File: ";

	public void SetFileName(string name)
	{
		fileNameText.text = $"{fileNamePrefix}{name}";
	}
}
