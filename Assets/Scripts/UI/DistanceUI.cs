using UnityEngine;
using UnityEngine.UI;
using CustomDataTypes;
using QuestSystem;
using TMPro;
using UIControllers;

public class DistanceUI : MonoBehaviour
{
	private const float GAME_UNITS_TO_DISTANCE_RATIO = 1f / 3f;
	private const string WAYPOINT_STRING = "{0}m", ZONE_STRING = "Zone: {0}";
	[SerializeField] private GameObject holder;
	[SerializeField] private TextMeshProUGUI zoneTextComponent;
	[SerializeField] private WaypointUIController waypointUI;
	[SerializeField] private Character mainChar;
	public static bool Hidden { get; set; }

	private void Update()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (Hidden)
		{
			holder.SetActive(false);
			return;
		}
		holder.SetActive(true);

		Vector3 charPos = CurrentPosition;
		Vector3 waypointPos = CharacterTargetWaypoint;
		waypointUI.Setup(charPos, waypointPos);

		int zone = Difficulty.DistanceBasedDifficulty(
			charPos.magnitude);
		zoneTextComponent.text = string.Format(ZONE_STRING, zone);
	}

	public void Activate(bool active)
	{
		zoneTextComponent.enabled = active;
	}

	private ChunkCoords CharacterCoordinates
		=> new ChunkCoords(CurrentPosition, EntityNetwork.CHUNK_SIZE);

	private Vector3 CurrentPosition
		=> mainChar.transform.position;

	private Vector3 CharacterTargetWaypoint => mainChar.GetWaypoint.Position;
}