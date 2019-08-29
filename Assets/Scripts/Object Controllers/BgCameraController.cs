using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BgCameraController : MonoBehaviour
{
	private Camera mainCam;
	private Camera MainCam { get { return mainCam ?? (mainCam = Camera.main); } }
	public float zoomStrength = 50f;
	private Camera cam;
	public Camera Cam { get { return cam ?? (cam = GetComponent<Camera>()); } }
	public const float SCROLL_SPEED = 0.3f;
	private CameraCtrl mainCamCtrl;
	private CameraCtrl MainCamCtrl { get { return mainCamCtrl ?? (mainCamCtrl = MainCam.GetComponent<CameraCtrl>()); } }

	private void Update()
	{
		float zoomLevel = MainCam.orthographicSize - (MainCamCtrl?.minCamSize ?? 0f);
		transform.position = new Vector3(MainCam.transform.position.x  * SCROLL_SPEED,
			MainCam.transform.position.y * SCROLL_SPEED,
			Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength + MainCam.transform.position.z));
	}
}