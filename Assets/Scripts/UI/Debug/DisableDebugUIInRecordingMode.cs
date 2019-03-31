using UnityEngine;

public class DisableDebugUIInRecordingMode : MonoBehaviour
{
	private DebugUI debugUI;
	private DebugUI DebugUi
	{
		get
		{
			return debugUI ?? (debugUI = GetComponentInChildren<DebugUI>(true));
		}
	}
	private RecordingModeController recordingModeController;
	private RecordingModeController Rmc
	{
		get
		{
			return recordingModeController
				?? (recordingModeController = Resources.Load<RecordingModeController>("RecordingModeSO"));
		}
	}
	private bool RecordingModeState
	{
		get
		{
			return Rmc?.RecordingMode ?? false;
		}
	}

	private void Update()
	{
		DebugUi?.gameObject.SetActive(!RecordingModeState);
	}
}