using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BgCameraController : MonoBehaviour
{
	public static BgCameraController bgCam;
	private Camera mainCam;
	public float zoomStrength = 50f;
	public Camera cam;

	private void Awake()
	{
		bgCam = this;
		cam = GetComponent<Camera>();
		mainCam = Camera.main;
	}

	private void Update()
	{
		float zoomLevel = mainCam.orthographicSize - CameraCtrl.camCtrl.MinCamSize;
		transform.localPosition = new Vector3(0f, 0f, Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength));
		//cam.fieldOfView = mainCam.orthographicSize;
	}
}