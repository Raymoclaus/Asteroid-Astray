using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
	public Camera Cam;
	[SerializeField]
	private CameraCtrlTracker trackerSO;
	public Transform targetToFollow;
	private Transform panView;
	public float panRange = 5f;
	public Entity followTarget;
	public float minCamSize = 1.7f;
	public ShakeEffect camShake;

	public static float CamSize { get; private set; }

	public float camZoomSpeed = 0.1f;
	public float camDrillZoomSpeed = 0.005f;
	public float camSizeModifier = 0.25f;
	private float zoomModifier = 1f;
	private Vector2 prevPos, aheadVector;
	[SerializeField]
	private float distanceAhead = 0.5f, moveAheadSpeed = 0.05f;
	public ChunkCoords coords;
	private List<ChunkCoords> coordsInView = new List<ChunkCoords>(16);

	public int TotalViewRange { get { return trackerSO.ENTITY_VIEW_RANGE + trackerSO.RangeModifier; } }

	public ChunkFiller chunkFiller;

	private void Start()
	{
		//get ref to Camera component
		Cam = Cam ?? GetComponent<Camera>();
		//if there is no target then create a default one
		targetToFollow = targetToFollow ?? new GameObject("camTarget").transform;
		//get ref to ChunkFiller component
		chunkFiller = chunkFiller ?? GetComponent<ChunkFiller>();
		//ensure the chunk filler has created chunks first
		chunkFiller.CheckForMovement();
		//get camera's coordinates on the grid
		coords = new ChunkCoords(transform.position);
		//get list of entities that are within the camera's view range
		GetEntitiesInView(coords);
		//start camera size at minimum size
		CamSize = minCamSize;
		//camera position starts ahead of the target
		aheadVector = new Vector2(Mathf.Sin(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad),
			Mathf.Cos(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad)) * distanceAhead;
	}

	private void Update()
	{
		UpdateSO();
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
			followTarget = FindObjectOfType<Entity>();
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
		chunkFiller.CheckForMovement();
		GetEntitiesInView(newCoords);
	}

	/// Sets position to be just above and ahead of the target
	private void FollowTarget()
	{
		if (targetToFollow != null)
		{
			Vector2 aheadTarget = new Vector2(Mathf.Sin(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad),
				Mathf.Cos(-targetToFollow.eulerAngles.z * Mathf.Deg2Rad)) * distanceAhead;
			if (panView != null)
			{
				aheadTarget = (panView.position - transform.position) / 2f;
			}
			float difference = Vector2.Distance(aheadTarget, aheadVector) / distanceAhead / 2f;
			aheadVector = Vector2.MoveTowards(aheadVector, aheadTarget, difference * distanceAhead * moveAheadSpeed
				* Time.deltaTime * 60f);
			transform.localPosition = targetToFollow.position + (Vector3)aheadVector + Vector3.forward * -1f;
		}
	}

	/// Zooms out based on the shuttles speed.
	private void AdjustSize()
	{
		if (followTarget.IsDrilling && !trackerSO.useConstantSize)
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
			float targetSize = followTarget.Rb.velocity.magnitude * camSizeModifier + minCamSize;
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
			if (trackerSO.useConstantSize)
			{
				targetSize = trackerSO.constantSize;
			}
			//calculation to determine how quickly the camera zoom needs to change
			float zoomDifference = targetSize - CamSize;
			float zoomDifferenceAbs = zoomDifference > 0 ? zoomDifference : -zoomDifference;
			float camZoomSpeedModifier = zoomDifferenceAbs > 1 && zoomDifference > 0 ? 1f : zoomDifferenceAbs;
			CamSize = Mathf.MoveTowards(CamSize, targetSize, camZoomSpeed * camZoomSpeedModifier
			* Time.deltaTime * 60f);
		} 

		//sets the camera size on the camera component
		Cam.orthographicSize = CamSize * zoomModifier;
		//ensures the ChunkFiller component fills a wide enough area to fill the camera's view
		chunkFiller.RangeIncrease = trackerSO.RangeModifier;
	}

	/// Disables all entities previously in view, gets a new list of entities and enables them.
	private void GetEntitiesInView(ChunkCoords? cc = null)
	{
		//if no coordinates are provided then just search around the center of the grid by default
		ChunkCoords center = cc ?? ChunkCoords.Zero;
		//keep track of coords previous in view
		//query the EntityNetwork for a list of coordinates in view based on camera's size
		List<ChunkCoords> newCoordsInView =
			EntityNetwork.GetCoordsInRange(center, trackerSO.ENTITY_VIEW_RANGE + trackerSO.RangeModifier);
		//create a lists and filter out coordinates that are still in view
		List<ChunkCoords> notInViewAnymore = new List<ChunkCoords>(coordsInView);
		List<ChunkCoords> newCoords = new List<ChunkCoords>(newCoordsInView);
		for (int i = notInViewAnymore.Count - 1; i >= 0; i--)
		{
			int index = newCoords.IndexOf(notInViewAnymore[i]);
			if (index != -1)
			{
				notInViewAnymore.RemoveAt(i);
				newCoords.RemoveAt(index);
			}
		}
		coordsInView = newCoordsInView;
		
		//disable all entities previously in view
		List<Entity> notInView = EntityNetwork.GetEntitiesAtCoords(notInViewAnymore);
		foreach (Entity e in notInView)
		{
			e.SetAllActivity(false);
		}

		//enables all entities now in view
		List<Entity> nowInView = EntityNetwork.GetEntitiesAtCoords(newCoords);
		foreach (Entity e in nowInView)
		{
			e.SetAllActivity(true);
		}

		CheckPhysicsRange(center);
	}

	private void CheckPhysicsRange(ChunkCoords center)
	{
		List<Entity> physicsRange = EntityNetwork.GetEntitiesInRange(center, Cnsts.MAX_PHYSICS_RANGE);

		foreach(Entity e in physicsRange)
		{
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
		trackerSO.useConstantSize = enable;
		trackerSO.constantSize = size;
	}

	private void UpdateSO()
	{
		if (!trackerSO)
		{
			print("Attach appropriate Scriptable Object tracker to " + GetType().Name);
		}

		trackerSO.camSize = CamSize;
		trackerSO.coords = coords;
		trackerSO.position = transform.position;
	}
}