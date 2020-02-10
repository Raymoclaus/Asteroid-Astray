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
			InstantOpen();
		}
		else
		{
			InstantClose();
		}
	}

	private void OnDestroy()
	{
		Pause.OnPause -= Open;
		Pause.OnResume -= Close;
	}

	public override void Open()
	{
		base.Open();
		StartCoroutine(TimedAction(Pause.SHIFT_DURATION,
			(float delta) => cGroup.alpha = delta,
			null));
	}

	public override void Close()
	{
		StartCoroutine(TimedAction(Pause.SHIFT_DURATION,
			(float delta) => cGroup.alpha = 1f - delta,
			base.Close));
	}

	protected void InstantOpen()
	{
		base.Open();
		cGroup.alpha = 1f;
	}

	protected void InstantClose()
	{
		cGroup.alpha = 0f;
		base.Close();
	}
}
