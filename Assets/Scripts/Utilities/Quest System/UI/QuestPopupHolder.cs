using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.UI
{
	public class QuestPopupHolder : MonoBehaviour
	{
		[SerializeField] private RectTransform rect;
		[SerializeField] private QuestNameUI nameUI;
		[SerializeField] private QuestRequirementUI requirementPrefab;
		[SerializeField] private CanvasGroup canvasGroup;

		private List<QuestRequirementUI> requirements = new List<QuestRequirementUI>();
		public Quest Quest { get; private set; }

		public void Setup(Quest q)
		{
			rect = rect ?? GetComponent<RectTransform>();
			Quest = q;
			Quest.OnQuestComplete += Deactivate;
			nameUI = nameUI ?? GetComponentInChildren<QuestNameUI>();
			nameUI.Setup(Quest);
			for (int i = 0; i < Quest.Requirements.Count; i++)
			{
				QuestRequirementUI req = Instantiate(requirementPrefab, transform, false);
				req.Setup(Quest.Requirements[i]);
				requirements.Add(req);
			}

			canvasGroup = canvasGroup ?? GetComponent<CanvasGroup>();
		}

		public void Deactivate(Quest quest)
		{
			if (Quest != quest) return;
			Quest.OnQuestComplete -= Deactivate;
			Destroy(gameObject);
		}
	}

}