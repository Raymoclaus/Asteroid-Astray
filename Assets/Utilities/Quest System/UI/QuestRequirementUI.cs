using TMPro;
using UnityEngine;

namespace QuestSystem.UI
{
	using UIControllers;

	public class QuestRequirementUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI textMesh;
		[SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		[SerializeField] private GameObject waypointUIHolder;
		[SerializeField] private WaypointUIController waypointUI;
		private const string DISTANCE_STRING = "{0}m";
		private QuestRequirement requirement;
		private Quester quester;

		private void LateUpdate()
		{
			if (requirement == null || quester == null) return;

			string targetWaypointID = requirement.WaypointID;
			IWaypoint targetWaypoint = WaypointManager.GetWaypointByID(targetWaypointID);
			if (requirement.Completed || targetWaypoint == null)
			{
				waypointUIHolder.SetActive(false);
				return;
			}
			waypointUIHolder.SetActive(true);

			Vector3 waypointPos = targetWaypoint.Position;
			Vector3 questerPos = quester.transform.position;
			waypointUI.Setup(questerPos, waypointPos);
		}

		public void Setup(QuestRequirement req, Quester quester)
		{
			requirement = req;
			requirement.OnQuestRequirementUpdated += UpdateRequirementDescription;
			requirement.OnQuestRequirementCompleted += Complete;
			SetText(requirement.GetDescription);
			this.quester = quester;
		}

		private void UpdateRequirementDescription()
		{
			SetText(requirement.GetDescription);
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
}