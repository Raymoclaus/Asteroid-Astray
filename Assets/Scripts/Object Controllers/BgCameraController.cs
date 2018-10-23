using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BgCameraController : MonoBehaviour
{
	public static BgCameraController bgCam;
	private Camera mainCam;
	public float zoomStrength = 50f;
	[HideInInspector]
	public Camera cam;
	public float scrollSpeed = 0.3f;

	private void Awake()
	{
		bgCam = this;
		mainCam = Camera.main;
		cam = GetComponent<Camera>();
	}

	private void Update()
	{
		float zoomLevel = mainCam.orthographicSize - CameraCtrl.singleton.minCameSize;
		transform.position = new Vector3(mainCam.transform.position.x  * scrollSpeed,
			mainCam.transform.position.y * scrollSpeed,
			Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength + mainCam.transform.position.z));
		//transform.localPosition = new Vector3(0f, 0f, Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength));
	}
}