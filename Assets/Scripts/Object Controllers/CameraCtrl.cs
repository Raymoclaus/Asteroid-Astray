using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
	public Camera Cam;
	public static CameraCtrl camCtrl;
	public Transform TargetToFollow;
	public Entity followTarget;
	public float MinCamSize = 2f;
	private static float _currentSize;

	public static float CamSize
	{
		get { return _currentSize; }
	}

	public float CamZoomSpeed = 0.05f;
	public float CamDrillZoomSpeed = 0.002f;
	public float CamSizeModifier = 5f;
	private Vector2 _prevPos, aheadVector;
	[SerializeField]
	private float distanceAhead = 0.5f, moveAheadSpeed = 0.2f;
	public ChunkCoords Coords;
	public const int EntityViewRange = 1;
	private List<ChunkCoords> _coordsInView = new List<ChunkCoords>(16);

	//roughly gets the range of network grid cells that the camera can see
	public static int RangeModifier
	{
		get { return (int)((_currentSize - camCtrl.MinCamSize) / 5f); }
	}

	public int TotalViewRange { get { return EntityViewRange + RangeModifier; } }

	public ChunkFiller CFiller;

	private void Awake()
	{
		camCtrl = this;
	}

	private void Start()
	{
		//get ref to Camera component
		Cam = Cam ?? GetComponent<Camera>();
		//if there is no target then create a default one
		TargetToFollow = TargetToFollow ?? new GameObject("camTarget").transform;
		//get ref to ChunkFiller component
		CFiller = CFiller ?? GetComponent<ChunkFiller>();
		//ensure the chunk filler has created chunks first
		CFiller.CheckForMovement();
		//get camera's coordinates on the grid
		Coords = new ChunkCoords(transform.position);
		//get list of entities that are within the camera's view range
		GetEntitiesInView(Coords);
		//start camera size at minimum size
		_currentSize = MinCamSize;
		//camera position starts ahead of the target
		aheadVector = new Vector2(Mathf.Sin(-TargetToFollow.eulerAngles.z * Mathf.Deg2Rad),
			Mathf.Cos(-TargetToFollow.eulerAngles.z * Mathf.Deg2Rad)) * distanceAhead;
	}

	private void Update()
	{
		//stay above target
		FollowTarget();
		//adjust orthographic size based on speed of target
		AdjustSize();

		//check if moved, ignore if no movement detected
		Vector2 pos = transform.position;
		if (_prevPos == pos) return;
		Moved(pos);
		_prevPos = pos;
	}

	/// Only called if position of the camera changes
	private void Moved(Vector2 newPos)
	{
		ChunkCoords newCc = new ChunkCoords(newPos);
		if (newCc == Coords) return;
		CoordsChanged(newCc);
		Coords = newCc;
	}

	/// Only called if the camera's coordinates change
	private void CoordsChanged(ChunkCoords newCoords)
	{
		CFiller.CheckForMovement();
		GetEntitiesInView(newCoords);
	}

	/// Sets position to be just above and ahead of the target
	private void FollowTarget()
	{
		if (TargetToFollow != null)
		{
			Vector2 aheadTarget = new Vector2(Mathf.Sin(-TargetToFollow.eulerAngles.z * Mathf.Deg2Rad),
				Mathf.Cos(-TargetToFollow.eulerAngles.z * Mathf.Deg2Rad)) * distanceAhead;
			float difference = Vector2.Distance(aheadTarget, aheadVector) / distanceAhead / 2f;
			aheadVector = Vector2.MoveTowards(aheadVector, aheadTarget, difference * distanceAhead * moveAheadSpeed);
			transform.position = TargetToFollow.position + (Vector3)aheadVector + TargetToFollow.forward * -0.4f;
		}
	}

	/// Zooms out based on the shuttles speed.
	private void AdjustSize()
	{
		if (followTarget.IsDrilling)
		{
			//gradually zoom in
			_currentSize = Mathf.MoveTowards(_currentSize, MinCamSize / 2f, CamDrillZoomSpeed);
		}
		else
		{
			//calculates the zoom level the camera should be at
			float targetSize = followTarget.Rb.velocity.magnitude * CamSizeModifier + MinCamSize;
			//calculation to determine how quickly the camera zoom needs to change
			float zoomDifference = targetSize - _currentSize;
			float zoomDifferenceAbs = zoomDifference > 0 ? zoomDifference : -zoomDifference;
			float camZoomSpeedModifier = zoomDifferenceAbs > 1 && zoomDifference > 0 ? 1f : zoomDifferenceAbs;
			_currentSize = Mathf.MoveTowards(_currentSize, targetSize, CamZoomSpeed * camZoomSpeedModifier);
		}
		//sets the camera size on the camera component
		Cam.orthographicSize = _currentSize;
		//ensures the ChunkFiller component fills a wide enough area to fill the camera's view
		CFiller.RangeIncrease = RangeModifier;
	}

	/// Disables all entities previously in view, gets a new list of entities and enables them.
	private void GetEntitiesInView(ChunkCoords? cc = null)
	{
		//if no coordinates are provided then just search around the center of the grid by default
		ChunkCoords center = cc ?? ChunkCoords.Zero;
		//keep track of coords previous in view
		//query the EntityNetwork for a list of coordinates in view based on camera's size
		List<ChunkCoords> newCoordsInView = EntityNetwork.GetCoordsInRange(center, EntityViewRange + RangeModifier);
		//create a lists and filter out coordinates that are still in view
		List<ChunkCoords> notInViewAnymore = new List<ChunkCoords>(_coordsInView);
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
		_coordsInView = newCoordsInView;
		
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
			if (!e.gameObject.activeSelf)
			{
				e.RepositionInNetwork();
			}
		}
	}

	public static bool IsCoordInView(ChunkCoords coord)
	{
		return ChunkCoords.MaxDistance(coord, camCtrl.Coords) <= EntityViewRange + RangeModifier;
	}

	public static bool IsCoordInPhysicsRange(ChunkCoords coord)
	{
		return ChunkCoords.MaxDistance(coord, camCtrl.Coords) < Cnsts.MAX_PHYSICS_RANGE;
	}
}