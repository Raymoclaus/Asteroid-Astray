using UnityEngine;

public class DisableDebugUIInRecordingMode : MonoBehaviour
{
	[SerializeField]
	private DebugUI debugUI;
	[SerializeField]
	private RecordingModeController recordingModeController;

	private void Update()
	{
		debugUI = debugUI ?? GetComponentInChildren<DebugUI>();
		if (!debugUI) return;

		debugUI.gameObject.SetActive(!recordingModeController.RecordingMode);
	}
}