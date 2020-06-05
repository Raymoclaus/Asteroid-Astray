using TMPro;
using UnityEngine;

namespace QuestSystem.UI
{
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

}