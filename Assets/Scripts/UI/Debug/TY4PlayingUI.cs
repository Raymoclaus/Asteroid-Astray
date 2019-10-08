using UnityEngine;
using InputHandler;

[RequireComponent(typeof(CanvasGroup))]
public class TY4PlayingUI : MonoBehaviour
{
	private CanvasGroup cGroup;
	private CanvasGroup CGroup => cGroup ?? (cGroup = GetComponent<CanvasGroup>());
	public bool active = false;
	public bool CanDisable = false;
	private Shuttle mainChar;
	private Shuttle MainChar => mainChar ?? (mainChar = FindObjectOfType<Shuttle>());

	private void Awake() => CGroup.alpha = active ? 1f : 0f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Y))
		{
			SetActive(!active);
		}

		if (active
			&& InputManager.GetInputDown("Scroll Dialogue")
			&& CGroup.alpha == 1f
			&& CanDisable)
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
