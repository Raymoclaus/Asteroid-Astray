using System.Collections.Generic;
using UnityEngine;

public class PopupUI : MonoBehaviour
{
	protected float popupHeight;
	[SerializeField]
	protected float scrollDelay = 3f, fullDelay = 5f;
	protected float scrollDelayTimer = 0f;
	[SerializeField]
	protected int popupViewLimit = 4;
	[SerializeField]
	protected float popupEntrySpeed = 2f, popupMoveSpeed = 5f;
	[SerializeField]
	protected LoadingTracker loadingTrackerSO;
	[SerializeField]
	protected RecordingModeController recordingModeTrackerSO;

	protected virtual void RemovePopup(int index)
	{

	}

	protected class PopupObject
	{
		public float timer;
		public RectTransform transform;

		public void SetTimer(float time)
		{
			timer = time;
		}

		public void AddTimer(float time)
		{
			timer += time;
		}
	}
}
