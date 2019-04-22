using UnityEngine;

public class TY4PlayingUI : MonoBehaviour
{
	[SerializeField] private CanvasGroup cGroup;
	private bool active = false;
	[SerializeField] private ShuttleTrackers shuttleTracker;

	private void Awake() => cGroup.alpha = active ? 1f : 0f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Y))
		{
			SetActive(!active);
		}

		if (active && InputHandler.GetInputDown(InputAction.ScrollDialogue) > 0f && cGroup.alpha == 1f)
		{
			SetActive(false);
		}

		cGroup.alpha = Mathf.MoveTowards(cGroup.alpha, active ? 1f : 0f, Time.unscaledDeltaTime);
	}

	public void SetActive(bool active)
	{
		if (active == this.active) return;
		if (Pause.IsStopped && !this.active) return;

		this.active = active;
		Pause.InstantPause(this.active);
		if (active)
		{
			shuttleTracker.canLaunch = true;
			shuttleTracker.canShoot = true;
		}
	}
}
