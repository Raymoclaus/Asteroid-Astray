using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BgCameraController : MonoBehaviour
{
	[SerializeField] private Camera foregroundCam;
	private Camera ForegroundCam { get { return foregroundCam ?? (foregroundCam = Camera.main); } }
	public float zoomStrength = 50f;
	private Camera cam;
	public Camera Cam { get { return cam ?? (cam = GetComponent<Camera>()); } }
	public const float SCROLL_SPEED = 0.3f;
	private CameraCtrl mainCamCtrl;
	private CameraCtrl MainCamCtrl { get { return mainCamCtrl ?? (mainCamCtrl = ForegroundCam.GetComponent<CameraCtrl>()); } }

	private void Update()
	{
		float zoomLevel = ForegroundCam.orthographicSize - (MainCamCtrl?.minCamSize ?? 0f);
		transform.position = new Vector3(ForegroundCam.transform.position.x  * SCROLL_SPEED,
			ForegroundCam.transform.position.y * SCROLL_SPEED,
			Mathf.Max(0.4f, 100f - zoomLevel * zoomStrength + ForegroundCam.transform.position.z));
	}
}