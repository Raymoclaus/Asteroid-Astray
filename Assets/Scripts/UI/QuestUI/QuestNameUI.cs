using UnityEngine;
using TMPro;

public class QuestNameUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI textMesh;

	public void Setup(Quest quest)
	{
		SetText(quest.Name);
	}

	private void SetText(string s)
	{
		textMesh.text = s;
	}
}
