using UnityEngine;

public class DisableDebugUIInRecordingMode : MonoBehaviour
{
	[SerializeField]
	private GameObject debugUI;

	private void Update()
	{
		debugUI.SetActive(!GameController.RecordingMode);
	}
}