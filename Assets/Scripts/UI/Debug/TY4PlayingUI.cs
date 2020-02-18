using UnityEngine;
using InputHandlerSystem;

[RequireComponent(typeof(CanvasGroup))]
public class TY4PlayingUI : MonoBehaviour
{
	private CanvasGroup cGroup;
	private CanvasGroup CGroup => cGroup ?? (cGroup = GetComponent<CanvasGroup>());
	public bool active = false;
	public bool CanDisable = false;
	private Character mainChar;
	private Character MainChar => mainChar ?? (mainChar = FindObjectOfType<Shuttle>());
	[SerializeField] private InputAction scrollDialogueAction;

	private void Awake() => CGroup.alpha = active ? 1f : 0f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Y))
		{
			SetActive(!active);
		}

		if (active
			&& InputManager.GetInputDown(scrollDialogueAction)
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
			MainChar.CanAttack = true;
		}
	}
}
