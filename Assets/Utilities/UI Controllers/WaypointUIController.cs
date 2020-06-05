using TMPro;
using UnityEngine;

namespace UIControllers
{
	public class WaypointUIController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI distanceText;
		[SerializeField] private RotationController arrow;
		private const string DISTANCE_STRING = "{0}m";
		private const float GAME_UNITS_TO_DISTANCE_RATIO = 1f / 3f;

		public void Setup(Vector3 startPosition, Vector3 goalPosition)
		{
			Vector3 vectorToGoal = goalPosition - startPosition;
			int distance = (int)(vectorToGoal.magnitude / GAME_UNITS_TO_DISTANCE_RATIO);
			distanceText.text = string.Format(DISTANCE_STRING, distance);

			float angle = Vector2.SignedAngle(Vector2.up, vectorToGoal);
			arrow.SetZRotation(angle);
		}
	}
}