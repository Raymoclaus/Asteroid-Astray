using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCtrl : MonoBehaviour
{
	[HideInInspector] private Camera cam = null;
	public Camera Cam { get { return cam ?? (cam = GetComponent<Camera>()); } }
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

	public int TotalViewRange {
		get { return ENTITY_VIEW_RANGE + RangeModifier; }
	}

	public ChunkFiller chunkFiller;

	//cache
	private List<ChunkCoords> notInViewAnymore = new List<ChunkCoords>();
	private List<ChunkCoords> newCoords = new List<ChunkCoords>();
	private List<Entity> notInView = new List<Entity>();
	private List<ChunkCoords> newCoordsInView = new List<ChunkCoords>();
	private List<Entity> nowInView = new List<Entity>();
	private List<Entity> physicsRange = new List<Entity>();

	private void Start()
	{
		//get ref to ChunkFiller component
		chunkFiller = chunkFiller ?? GetComponent<ChunkFiller>();
		//get camera's coordinates on the grid
		coords = new ChunkCoords(transform.position);
		//get list of entities that are within the camera's view range
		GetEntitiesInView(coords);
		//start camera size at minimum size
		CamSize = minCamSize;
		//default follow target to shuttle if no target is set
		followTarget = followTarget ?? FindObjectOfType<Shuttle>();
	}

	private void Update()
	{
		if (followTarget)
		{
			targetToFollow = followTarget.transform;
			//stay above target
			FollowTarget();
			//adjust orthographic size based on speed of target
			AdjustSize();
		}
		else
		{
			//if follow target does not exist, find one
			followTarget = FindObjectOfType<Character>();
			if (followTarget != null)
			{
				targetToFollow = followTarget.transform;
			}
		}

		//check if moved, ignore if no movement detected
		Vector2 pos = transform.position;
		if (prevPos == pos) return;
		Moved(pos);
		prevPos = pos;
	}

	/// Only called if position of the camera changes
	private void Moved(Vector2 newPos)
	{
		ChunkCoords newCc = new ChunkCoords(newPos);
		if (newCc == coords) return;
		CoordsChanged(newCc);
		coords = newCc;
	}

	/// Only called if the camera's coordinates change
	private void CoordsChanged(ChunkCoords newCoords)
	{
		GetEntitiesInView(newCoords);
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

	/// Zooms out based on the shuttles speed.
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
	private void GetEntitiesInView(ChunkCoords? cc = null)
	{
		//if no coordinates are provided then just search around the center of the grid by default
		ChunkCoords center = cc ?? ChunkCoords.Zero;
		//keep track of coords previous in view
		//query the EntityNetwork for a list of coordinates in view based on camera's size
		newCoordsInView.Clear();
		EntityNetwork.GetCoordsInRange(center, ENTITY_VIEW_RANGE + RangeModifier, newCoordsInView);
		//create a lists and filter out coordinates that are still in view
		notInViewAnymore.Clear();
		newCoords.Clear();
		notInViewAnymore.AddRange(coordsInView);
		newCoords.AddRange(newCoordsInView);
		for (int i = notInViewAnymore.Count - 1; i >= 0; i--)
		{
			int index = -1;
			for (int j = 0; j < newCoords.Count; j++)
			{
				if (newCoords[j] == notInViewAnymore[i])
				{
					index = j;
					break;
				}
			}
			if (index != -1)
			{
				notInViewAnymore.RemoveAt(i);
				newCoords.RemoveAt(index);
			}
		}
		coordsInView = newCoordsInView;
		
		//disable all entities previously in view
		notInView.Clear();
		EntityNetwork.GetEntitiesAtCoords(notInViewAnymore, addToList: notInView);
		for (int i = 0; i < notInView.Count; i++)
		{
			Entity e = notInView[i];
			e.SetAllActivity(false);
		}

		//enables all entities now in view
		nowInView.Clear();
		EntityNetwork.GetEntitiesAtCoords(newCoords, addToList: nowInView);
		for (int i = 0; i < nowInView.Count; i++)
		{
			Entity e = nowInView[i];
			e.SetAllActivity(true);
		}

		CheckPhysicsRange(center);
	}

	private void CheckPhysicsRange(ChunkCoords center)
	{
		physicsRange.Clear();
		EntityNetwork.GetEntitiesInRange(center, Constants.MAX_PHYSICS_RANGE,
			addToList: physicsRange);

		for (int i = 0; i < physicsRange.Count; i++)
		{
			Entity e = physicsRange[i];
			if (e && !e.gameObject.activeSelf)
			{
				e.RepositionInNetwork();
			}
		}
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
		return ChunkCoords.MaxDistance(coord, coords) <= ENTITY_VIEW_RANGE + RangeModifier;
	}

	public bool IsCoordInPhysicsRange(ChunkCoords coord)
	{
		return ChunkCoords.MaxDistance(coord, coords) < Constants.MAX_PHYSICS_RANGE;
	}

	public void Zoom(float zoomLevel)
	{
		zoomModifier = zoomLevel;
	}
}