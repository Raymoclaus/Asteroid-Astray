using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgCameraController : MonoBehaviour
{
	private Camera mainCam;
	public float zoomStrength = 2f;

	private void Awake()
	{
		mainCam = Camera.main;
	}

	private void Update()
	{
		float zoomLevel = mainCam.orthographicSize - 1.7f;
		transform.localPosition = new Vector3(0f, 0f, Mathf.Max(10f, 300f - zoomLevel * zoomStrength));
	}
}