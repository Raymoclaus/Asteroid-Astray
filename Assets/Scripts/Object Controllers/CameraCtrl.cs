using CustomDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCtrl : MonoBehaviour
{
	[HideInInspector] private Camera cam;
	public Camera Cam => cam ?? (cam = GetComponent<Camera>());
	[SerializeField] private Transform targetToFollow;
	private Transform panView;
	public float panRange = 5f;
	public Character followTarget;
	public float minCamSize = 1.7f;
	public ShakeEffect camShake;

	public static float CamSize { get; private set; }

	public float camZoomSpeed = 0.1f;
	public float camDrillZoomSpeed = 0.005f;
	public float camSizeModifier = 0.25f;
	private float zoomModifier = 1f;
	private Vector2 prevPos, aheadVector;
	[SerializeField] private float distanceAhead = 0.5f, moveAheadSpeed = 0.05f;
	public ChunkCoords coords;
	private List<ChunkCoords> coordsInView = new List<ChunkCoords>(16);
	private bool useConstantSize = false;
	private bool useLookAheadModifier = false;
	private const int ENTITY_VIEW_RANGE = 1;
	public int RangeModifier { get { return (int)((Cam.orthographicSize - minCamSize) / 5f); } }
	private float distanceAheadModifier = 2f;
	private float moveAheadSpeedModifier = 2f;
	private float constantSize = 2f;

	public int TotalViewRange => ENTITY_VIEW_RANGE + RangeModifier;

	public ChunkFiller chunkFiller;

	private void Start()
	{
		//get ref to ChunkFiller component
		chunkFiller = chunkFiller ?? GetComponent<ChunkFiller>();
		//get camera's coordinates on the grid
		coords = new ChunkCoords(transform.position, EntityNetwork.CHUNK_SIZE);
		//start camera size at minimum size
		CamSize = minCamSize;
		
		LoadingController.AddListener(Initialise);
	}

	private void Initialise()
	{
		//get list of entities that are within the camera's view range
		UpdateEntitiesInView(coords, ChunkCoords.Invalid);
		//default follow target to main character if no target is set
		followTarget = followTarget ?? NarrativeManager.MainCharacter;
	}

	private void Update()
	{
		if (followTarget == null) return;

		targetToFollow = followTarget.transform;
		//stay above target
		FollowTarget();
		//adjust orthographic size based on speed of target
		AdjustSize();

		//check if moved, ignore if no movement detected
		Vector2 pos = transform.position;
		if (prevPos == pos) return;
		Moved(pos);
		prevPos = pos;
	}

	/// Only called if position of the camera changes
	private void Moved(Vector2 newPos)
	{
		ChunkCoords newCc = new ChunkCoords(newPos, EntityNetwork.CHUNK_SIZE);
		if (newCc == coords) return;
		CoordsChanged(newCc, coords);
	}

	/// Only called if the camera's coordinates change
	private void CoordsChanged(ChunkCoords newCoords, ChunkCoords oldCoords)
	{
		coords = newCoords;
		if (!EntityNetwork.IsReady) return;
		UpdateEntitiesInView(newCoords, oldCoords);
	}

	/// Sets position to be just above and ahead of the target
	private void FollowTarget()
	{
		if (targetToFollow != null)
		{
			Vector2 aheadTarget = new Vector2(
				Mathf.Sin(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad),
				Mathf.Cos(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad)) * distanceAhead
				* (useLookAheadModifier ? distanceAheadModifier : 1f);
			if (panView != null)
			{
				aheadTarget = (panView.position - transform.position) / 2f;
			}
			float difference = Vector2.Distance(aheadTarget, aheadVector) / distanceAhead / 2f;
			aheadVector = Vector2.MoveTowards(aheadVector, aheadTarget,
				difference * distanceAhead * moveAheadSpeed
				* (useLookAheadModifier ? moveAheadSpeedModifier : 1f)
				* Time.deltaTime * 60f);
			transform.localPosition = targetToFollow.position
				+ (Vector3)aheadVector
				+ Vector3.forward * -1f;
		}
	}

	/// Zooms out based on the target's speed.
	private void AdjustSize()
	{
		if (followTarget.IsDrilling && !useConstantSize)
		{
			//gradually zoom in
			float difference = Mathf.Abs(CamSize - minCamSize / 2f);
			float zoomSpeedModifier = Mathf.Min(1f, difference);
			CamSize = Mathf.MoveTowards(CamSize, minCamSize / 2f,
				camDrillZoomSpeed * zoomSpeedModifier * Time.deltaTime * 60f);
		}
		else
		{
			//calculates the zoom level the camera should be at
			float targetSize = followTarget.rb.velocity.magnitude * camSizeModifier + minCamSize;
			if (panView != null)
			{
				float dist = Vector2.Distance(panView.position, targetToFollow.position) / 2f;
				if (dist > 0f)
				{
					targetSize += dist;
					if (dist > panRange)
					{
						panView = null;
					}
				}
			}
			if (useConstantSize)
			{
				targetSize = constantSize;
			}
			//calculation to determine how quickly the camera zoom needs to change
			float zoomDifference = targetSize - CamSize;
			float zoomDifferenceAbs = zoomDifference > 0 ? zoomDifference : -zoomDifference;
			float camZoomSpeedModifier = zoomDifferenceAbs > 1 && zoomDifference > 0 ?
				1f : zoomDifferenceAbs;
			CamSize = Mathf.MoveTowards(CamSize, targetSize, camZoomSpeed * camZoomSpeedModifier
			* Time.deltaTime * 60f);
		} 

		//sets the camera size on the camera component
		Cam.orthographicSize = CamSize * zoomModifier;
		//ensures the ChunkFiller component fills a wide enough area to fill the camera's view
		chunkFiller.RangeIncrease = RangeModifier;
	}
	
	/// Disables all entities previously in view, gets a new list of entities and enables them.
	private void UpdateEntitiesInView(ChunkCoords newCoords, ChunkCoords oldCoords)
	{
		//keep track of coords previous in view
		//query the EntityNetwork for a list of coordinates in view based on camera's size
		int range = ENTITY_VIEW_RANGE + RangeModifier;
		EntityNetwork.IterateEntitiesInRange(
			oldCoords,
			range,
			e =>
			{
				ChunkCoords eCC = e.GetCoords();
				if (!IsInRange(newCoords, eCC, range)
				    || !newCoords.IsValid()
				    || !eCC.IsValid())
				{
					e.RepositionInNetwork(true);
				}

				return false;
			});

		EntityNetwork.IterateEntitiesInRange(
			newCoords,
			range,
			e =>
			{
				ChunkCoords eCC = e.GetCoords();
				if (IsInRange(oldCoords, eCC, range)
				    || !oldCoords.IsValid()
				    || !eCC.IsValid())
				{
					e.RepositionInNetwork(true);
				}

				return false;
			});
		
		CheckPhysicsRange(newCoords, oldCoords);
	}

	private void CheckPhysicsRange(ChunkCoords newCoords, ChunkCoords oldCoords)
	{
		int range = Constants.MAX_PHYSICS_RANGE;
		EntityNetwork.IterateEntitiesInRange(
			oldCoords,
			range,
			e =>
			{
				ChunkCoords eCC = e.GetCoords();
				if (!IsInRange(newCoords, eCC, range)
				    || !newCoords.IsValid()
				    || !eCC.IsValid())
				{
					e.RepositionInNetwork(true);
				}

				return false;
			});

		EntityNetwork.IterateEntitiesInRange(
			newCoords,
			range,
			e =>
			{
				ChunkCoords eCC = e.GetCoords();
				if (IsInRange(oldCoords, eCC, range)
				    || !oldCoords.IsValid()
				    || !eCC.IsValid())
				{
					e.RepositionInNetwork(true);
				}

				return false;
			});
	}

	public void CamShake()
	{
		camShake.Begin(0.1f, 0f, 0.1f);
	}

	public void QuickZoom(float zoomPercentage = 0.8f, float time = 0.5f, bool unscaledTime = true)
	{
		StartCoroutine(QuickZoomCoroutine(zoomPercentage, time, unscaledTime));
	}

	private IEnumerator QuickZoomCoroutine(float zoomPercentage, float time, bool unscaledTime)
	{
		zoomModifier = zoomPercentage;
		while (time >= 0f)
		{
			time -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			yield return null;
		}
		zoomModifier = 1f;
		CamSize = Cam.orthographicSize;
	}

	public void Pan(Transform panTarget)
	{
		panView = panTarget;
	}

	public Transform GetPanTarget()
	{
		return panView;
	}

	public void SetConstantSize(bool enable = true, float size = 2f)
	{
		useConstantSize = enable;
		constantSize = size;
	}

	public void SetLookAheadDistance(bool enable = true, float distanceModifier = 2f, float speedModifier = 2f)
	{
		useLookAheadModifier = enable;
		distanceAheadModifier = distanceModifier;
		moveAheadSpeedModifier = speedModifier;
	}

	public bool IsCoordInView(ChunkCoords coord)
	{
		return IsInRange(coords, coord, ENTITY_VIEW_RANGE + RangeModifier);
	}

	public bool IsCoordInPhysicsRange(ChunkCoords coord)
	{
		return IsInRange(coords, coord, Constants.MAX_PHYSICS_RANGE);
	}

	private bool IsInRange(ChunkCoords center, ChunkCoords check, int range)
	{
		return ChunkCoords.SquareDistance(center, check) <= range;
	}

	public void Zoom(float zoomLevel)
	{
		zoomModifier = zoomLevel;
	}

	public bool IsInView(GameObject obj)
	{
		Vector3 viewportPoint = cam.WorldToViewportPoint(obj.transform.position);
		return viewportPoint.x >= 0f
		       && viewportPoint.x <= 1f
		       && viewportPoint.y >= 0f
		       && viewportPoint.y <= 1f;
	}
}