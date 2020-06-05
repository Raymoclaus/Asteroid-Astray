using CustomDataTypes;
using StatisticsTracker;
using TMPro;
using UIControllers;
using UnityEngine;

public class DistanceUI : MonoBehaviour
{
	private static DistanceUI _instance;

	private const float GAME_UNITS_TO_DISTANCE_RATIO = 1f / 3f;
	private const string WAYPOINT_STRING = "{0}m", ZONE_STRING = "Zone: {0}";
	[SerializeField] private GameObject holder;
	[SerializeField] private TextMeshProUGUI zoneTextComponent;
	[SerializeField] private WaypointUIController waypointUI;
	[SerializeField] private BoolStatTracker visibilityTracker;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Update()
	{
		UpdateText();
	}

	public static bool IsHidden
	{
		get => _instance.visibilityTracker.Value == false;
		set => _instance.visibilityTracker.SetValue(!value);
	}

	private void UpdateText()
	{
		if (IsHidden)
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

	private Character MainCharacter => NarrativeManager.MainCharacter;

	private Vector3 CurrentPosition
	{
		get
		{
			if (MainCharacter == null) return Vector3.zero;
			return MainCharacter.Position;
		}
	}

	private Vector3 CharacterTargetWaypoint
	{
		get
		{
			if (MainCharacter == null
			    || MainCharacter.GetTargetWaypoint == null) return Vector3.zero;
			return MainCharacter.GetTargetWaypoint.Position;
		}
	}
}