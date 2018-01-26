using System.Collections.Generic;
using UnityEngine;

namespace Object_Controllers
{
	public class CameraCtrl : MonoBehaviour
	{
		public Camera Cam;
		public Transform TargetToFollow;
		public Shuttle Shuttle;
		public Transform ShuttleTransform;
		private const float MinCamSize = 2f;
		private static float _currentSize = MinCamSize;

		public static float CamSize
		{
			get { return _currentSize; }
		}

		public float CamZoomSpeed = 0.05f;
		public float CamSizeModifier = 5f;
		private Vector2 _prevPos;
		public ChunkCoords Coords;
		private List<Entity> _entitiesInView = new List<Entity>(300);
		private const int EntityViewRange = 1;

		//roughly gets the range of network grid cells that the camera can see
		private static int RangeModifier
		{
			get { return (int) ((_currentSize - MinCamSize) / 5f); }
		}

		public ChunkFiller CFiller;

		
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

		/// Sets position to be just above the target
		private void FollowTarget()
		{
			if (TargetToFollow != null)
			{
				transform.position = TargetToFollow.position + TargetToFollow.forward * -10f;
			}
		}

		/// Zooms out based on the shuttles speed.
		private void AdjustSize()
		{
			//only zooms the camera out if the target the camera is following is the shuttle
			if (TargetToFollow == ShuttleTransform && Shuttle != null)
			{
				//calculates the zoom level the camera should be at
				float targetSize = Shuttle.Rb.velocity.magnitude * CamSizeModifier * Cnsts.TimeSpeed + MinCamSize;
				//calculation to determine how quickly the camera zoom needs to change
				float zoomDifference = targetSize - _currentSize;
				float zoomDifferenceAbs = zoomDifference > 0 ? zoomDifference : -zoomDifference;
				float camZoomSpeedModifier = zoomDifferenceAbs > 1 && zoomDifference > 0 ? 1f : zoomDifferenceAbs;
				_currentSize = Mathf.MoveTowards(_currentSize, targetSize, CamZoomSpeed * camZoomSpeedModifier * Cnsts.TimeSpeed);
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
			//query the EntityNetwork for a list of entities around those coordinates based on camera's size
			List<Entity> nowInView = EntityNetwork.GetEntitiesInRange(center, EntityViewRange + RangeModifier);

			//disable all entities previously in view
			foreach (Entity e in _entitiesInView)
			{
				e.SetAllActivity(false);
			}

			//enables all entities now in view
			foreach (Entity e in nowInView)
			{
				e.SetAllActivity(true);
			}

			//assigns new list to existing field
			_entitiesInView = nowInView;
		}
	}
}