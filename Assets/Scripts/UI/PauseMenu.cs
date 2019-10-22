using System.Collections;
using UnityEngine;
using TabbedMenuSystem;

public class PauseMenu : TabbedMenuController
{
	[SerializeField] private CanvasGroup cGroup;

	private void Awake()
	{
		Pause.OnPause += Open;
		Pause.OnResume += Close;

		if (Pause.IsPaused)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	protected override void Open()
	{
		base.Open();
		StartCoroutine(TimedAction(Pause.SHIFT_DURATION,
			(float delta) => cGroup.alpha = delta,
			null));
	}

	protected override void Close()
	{
		StartCoroutine(TimedAction(Pause.SHIFT_DURATION,
			(float delta) => cGroup.alpha = 1f - delta,
			base.Close));
	}
}
