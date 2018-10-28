using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BgCameraController : MonoBehaviour
{
	private Camera mainCam;
	public float zoomStrength = 50f;
	[SerializeField]
	private Camera cam;
	public float scrollSpeed = 0.3f;
	[SerializeField]
	private CameraCtrlTracker camTrackerSO;
	[SerializeField]
	private BgCamTracker trackerSO;

	private void Awake()
	{
		mainCam = Camera.main;
		cam = cam ?? GetComponent<Camera>();
	}

	private void Update()
	{
		UpdateSO();
		float zoomLevel = mainCam.orthographicSize - (camTrackerSO ? camTrackerSO.minCamSize : 0f);
		transform.position = new Vector3(mainCam.transform.position.x  * scrollSpeed,
			mainCam.transform.position.y * scrollSpeed,
			Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength + mainCam.transform.position.z));
	}

	private void UpdateSO()
	{
		if (!trackerSO)
		{
			print("Attach appropriate Scriptable Object tracker to " + GetType().Name);
			return;
		}

		cam.fieldOfView = trackerSO.fieldOfView;
		trackerSO.position = transform.position;
	}
}