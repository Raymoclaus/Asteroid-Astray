using InputHandlerSystem;
using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TY4PlayingUI : MonoBehaviour
{
	private CanvasGroup cGroup;
	private CanvasGroup CGroup => cGroup ?? (cGroup = GetComponent<CanvasGroup>());
	public bool active = false;
	public bool CanDisable = false;
	[SerializeField] private GameAction scrollDialogueAction;

	private void Awake() => CGroup.alpha = active ? 1f : 0f;

	private void OnEnable()
	{
		if (NarrativeManager.MainCharacter == null)
		{
			//subscribe so we know when a main character is selected
			Action callback = null;
			callback = () =>
			{
				Character c = NarrativeManager.MainCharacter;
				if (c == null) return;
				c.OnDestroyed += e => SetActive(true);
				NarrativeManager.OnMainCharacterUpdated -= callback;
			};
			NarrativeManager.OnMainCharacterUpdated += callback;
		}
	}

	private void OnDisable()
	{

	}

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
		TimeController.SetTimeScale(this, this.active ? 0f : 1f);
		if (active)
		{
			NarrativeManager.MainCharacter.CanAttack = true;
		}
	}
}
