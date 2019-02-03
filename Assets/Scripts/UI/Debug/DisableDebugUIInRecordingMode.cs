using UnityEngine;

public class DisableDebugUIInRecordingMode : MonoBehaviour
{
	[SerializeField]
	private DebugUI debugUI;
	[SerializeField]
	private RecordingModeController recordingModeController;
	private bool recordingModeState = false;

	private void Awake()
	{
		recordingModeState = recordingModeController.RecordingMode;
	}

	private void Update()
	{
		debugUI = debugUI ?? GetComponentInChildren<DebugUI>();
		if (!debugUI || recordingModeController.RecordingMode == recordingModeState) return;

		debugUI.gameObject.SetActive(!recordingModeController.RecordingMode);
		recordingModeState = recordingModeController.RecordingMode;
	}
}