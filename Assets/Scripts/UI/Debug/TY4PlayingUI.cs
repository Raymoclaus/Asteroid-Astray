using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TY4PlayingUI : MonoBehaviour
{
	private CanvasGroup cGroup;
	private CanvasGroup CGroup
	{
		get { return cGroup ?? (cGroup = GetComponent<CanvasGroup>()); }
	}
	private bool active = false;
	private Shuttle mainChar;
	private Shuttle MainChar
	{
		get { return mainChar ?? (mainChar = FindObjectOfType<Shuttle>()); }
	}

	private void Awake() => CGroup.alpha = active ? 1f : 0f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Y))
		{
			SetActive(!active);
		}

		if (active && InputHandler.GetInputDown(InputAction.ScrollDialogue) > 0f && CGroup.alpha == 1f)
		{
			SetActive(false);
		}

		CGroup.alpha = Mathf.MoveTowards(CGroup.alpha, active ? 1f : 0f, Time.unscaledDeltaTime);
	}

	public void SetActive(bool active)
	{
		if (active == this.active) return;

		this.active = active;
		Pause.InstantPause(this.active);
		if (active)
		{
			MainChar.CanLaunch = true;
			MainChar.CanShoot = true;
		}
	}
}
