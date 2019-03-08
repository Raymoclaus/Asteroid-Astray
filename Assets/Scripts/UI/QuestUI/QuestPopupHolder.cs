using System.Collections.Generic;
using UnityEngine;

public class QuestPopupHolder : MonoBehaviour
{
	[SerializeField] private RectTransform rect;
	[SerializeField] private QuestNameUI nameUI;
	[SerializeField] private QuestRequirementUI requirementPrefab;
	[SerializeField] private CanvasGroup canvasGroup;

	private List<QuestRequirementUI> requirements = new List<QuestRequirementUI>();
	private Quest quest;

	public void Setup(Quest q)
	{
		rect = rect ?? GetComponent<RectTransform>();
		quest = q;
		nameUI = nameUI ?? GetComponentInChildren<QuestNameUI>();
		nameUI.Setup(quest);
		for (int i = 0; i < quest.Requirements.Count; i++)
		{
			QuestRequirementUI req = Instantiate(requirementPrefab, transform, false);
			req.Setup(quest.Requirements[i]);
			requirements.Add(req);
		}

		canvasGroup = canvasGroup ?? GetComponent<CanvasGroup>();
	}

	public void Deactivate(Quest q)
	{
		Destroy(gameObject);
	}
}
