using UnityEngine;
using TMPro;

public class QuestRequirementUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI textMesh;
	[SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
	private QuestRequirement requirement;

	public void Setup(QuestRequirement req)
	{
		requirement = req;
		requirement.OnQuestRequirementUpdated += UpdateRequirementDescription;
		requirement.OnQuestRequirementCompleted += Complete;
		SetText(requirement.GetDescription());
	}

	private void UpdateRequirementDescription()
	{
		SetText(requirement.GetDescription());
	}

	private void SetText(string s)
	{
		textMesh.text = s;
	}

	public void Complete()
	{
		textMesh.color = completedColor;
		textMesh.fontStyle |= FontStyles.Strikethrough;
	}
}
