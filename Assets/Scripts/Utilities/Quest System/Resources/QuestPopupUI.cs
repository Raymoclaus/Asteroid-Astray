using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.UI
{
	public class QuestPopupUI : PopupUI
	{
		private static event Action<Quester> OnQuesterSet;
		private static Quester quester;
		private static QuestPopupUI instance;
		[SerializeField] private QuestPopupHolder popupPrefab;
		private List<QuestPopupHolder> popups = new List<QuestPopupHolder>();

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

			SetUpNewQuester(quester);
			OnQuesterSet += SetUpNewQuester;
		}

		private void OnDestroy()
		{
			OnQuesterSet -= SetUpNewQuester;
		}

		public static void SetQuester(Quester newQuester)
		{
			if (newQuester == null) return;
			quester = newQuester;
			OnQuesterSet?.Invoke(newQuester);
		}

		private void SetUpNewQuester(Quester newQuester)
		{
			if (newQuester == null) return;
			ShowQuest(newQuester.TopPriorityQuest);
			quester.OnTopPriorityQuestSet += ShowQuest;
		}

		private void ShowQuest(Quest quest)
		{
			if (quest == null) return;
			GeneratePopup(quest);
		}

		protected virtual void GeneratePopup(Quest quest)
		{
			QuestPopupHolder popup = Instantiate(popupPrefab, transform, false);
			popups.Add(popup);
			popup.Setup(quest);
		}

		private QuestPopupHolder GetPopupForQuest(Quest quest)
			=> popups.First(t => t.Quest == quest);
	}

}