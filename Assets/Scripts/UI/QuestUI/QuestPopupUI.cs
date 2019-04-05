using UnityEngine;

public class QuestPopupUI : PopupUI
{
	private static QuestPopupUI instance;
	[SerializeField] private QuestPopupHolder popupPrefab;
	private QuestPopupHolder currentPopup;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	public static void ShowQuest(Quest quest)
	{
		instance.GeneratePopup(quest);
	}

	public virtual void GeneratePopup(Quest quest)
	{
		currentPopup = Instantiate(popupPrefab, transform, false);
		currentPopup.Setup(quest);
		quest.OnQuestComplete += currentPopup.Deactivate;
	}
}
