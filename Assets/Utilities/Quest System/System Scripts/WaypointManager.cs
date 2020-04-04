using System.Collections.Generic;
using TriggerSystem;
using UnityEngine;

namespace QuestSystem
{
	public class WaypointManager : MonoBehaviour
	{
		private static WaypointManager m_instance;
		[SerializeField] private VicinityWaypoint vicinityWaypointPrefab;
		[SerializeField] private AttachableWaypoint attachableWaypointPrefab;

		//unique ID, waypoint
		private static Dictionary<string, IWaypoint> waypoints = new Dictionary<string, IWaypoint>();
		private static Transform m_waypointParent;

		private static Transform WaypointParent => m_waypointParent != null
			? m_waypointParent
			: (m_waypointParent = new GameObject("Waypoints").transform);

		private void Awake()
		{
			if (m_instance == null)
			{
				m_instance = this;
			}

			if (m_instance != this)
			{
				Destroy(gameObject);
				return;
			}
		}

		private void OnDestroy()
		{
			waypoints.Clear();
		}

		/// <summary>
		/// Adds a waypoint to the tracked list of waypoints.
		/// If the waypoint already has an ID that exists in the list, it will not be added, even if the waypoint itself is not in the list.
		/// </summary>
		/// <param name="waypoint"></param>
		public static void AddWaypoint(IWaypoint waypoint)
		{
			if (IDExists(waypoint.UniqueID)) return;

			if (waypoint.UniqueID == null)
			{
				UniqueIDGenerator.AddObject(waypoint);
			}
			waypoints.Add(waypoint.UniqueID, waypoint);
		}

		public static void RemoveWaypoint(IWaypoint waypoint)
		{
			RemoveWaypointByID(waypoint.UniqueID);
		}

		public static void RemoveWaypointByID(string ID)
		{
			if (!IDExists(ID)) return;
			waypoints.Remove(ID);
		}

		private static bool IDExists(string ID)
		{
			return ID != null && waypoints.ContainsKey(ID);
		}

		public static IWaypoint GetWaypointByID(string ID)
		{
			if (!IDExists(ID)) return null;
			return waypoints[ID];
		}

		public static VicinityWaypoint CreateWaypoint(Vector3 position, float radius, IActor expectedActor)
		{
			//create a new vicinity waypoint
			VicinityWaypoint wp = Instantiate(m_instance.vicinityWaypointPrefab);
			//hold it under a parent transform
			wp.transform.parent = WaypointParent;
			//set position
			wp.Position = position;
			//set radius
			wp.Radius = radius;
			//set expected actor
			wp.ExpectedActor = expectedActor;
			//add waypoint to tracked list of waypoints
			AddWaypoint(wp);

			return wp;
		}

		public static AttachableWaypoint CreateAttachableWaypoint(IWaypointable waypointable, float radius, IActor expectedActor)
		{
			//create a attachable waypoint
			AttachableWaypoint wp = Instantiate(m_instance.attachableWaypointPrefab);
			//hold it under a parent transform
			wp.transform.parent = WaypointParent;
			//attach it to a waypointable object
			wp.AttachedWaypointable = waypointable;
			//set radius
			wp.Radius = radius;
			//set expected actor
			wp.ExpectedActor = expectedActor;
			//add waypoint to tracked list of waypoints
			AddWaypoint(wp);

			return wp;
		}
	} 
}
