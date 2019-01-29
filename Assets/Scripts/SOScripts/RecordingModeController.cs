using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/RecordingModeController")]
public class RecordingModeController : ScriptableObject
{
	[SerializeField]
	private bool recordingMode = false;
	public bool RecordingMode { get { return recordingMode; } }
	public float UnscaledDeltaTime { get { return RecordingMode ? 1.4f / 60f : Time.unscaledDeltaTime; } }
	public AnimationClip drillLaunchLightningEffect;

	private void UpdateRecordModeFixes(RecordingModeController fixes)
	{
		if (!fixes) return;

		if (fixes.drillLaunchLightningEffect)
		{
			fixes.drillLaunchLightningEffect.frameRate = recordingMode ? 24f * (1f / Time.deltaTime) / 60f : 24f;
		}
	}

	public void SetRecordingMode(bool recording, RecordingModeController fixes)
	{
		if (recording == recordingMode) return;

		recordingMode = recording;
		UpdateRecordModeFixes(fixes);
	}
}