using UnityEngine;

public class QuestPopupUI : PopupUI
{
	[SerializeField] private QuestPopupHolder popupPrefab;
	private QuestPopupHolder currentPopup;

	public virtual void GeneratePopup(Quest quest)
	{
		currentPopup = Instantiate(popupPrefab, transform, false);
		currentPopup.Setup(quest);
		quest.OnQuestComplete += currentPopup.Deactivate;
	}
}
