using UnityEngine;
using TabbedMenuSystem;
using InputHandlerSystem;
using System;

public class PauseMenu : TabbedMenuController
{
	public static event Action OnStartedOpening, OnFinishedOpening, OnStartedClosing, OnFinishedClosing;

	[SerializeField] private CanvasGroup cGroup;
	[SerializeField] private GameAction _pauseAction;
	[SerializeField] private float _shiftDuration = 0.5f;

	private void Awake()
	{
		InstantClose();
	}

	private void Update()
	{
		if (IsShifting) return;

		if (InputManager.GetInputDown(_pauseAction))
		{
			if (IsOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	public override void Open()
	{
		base.Open();
		IsShifting = true;
		OnStartedOpening?.Invoke();

		StartCoroutine(TimedAction(_shiftDuration,
			delta =>
			{
				TimeController.SetTimeScale(this, 1f - delta);
				cGroup.alpha = delta;
			},
			() =>
			{
				IsShifting = false;
				OnFinishedOpening?.Invoke();
			}));
	}

	public override void Close()
	{
		IsShifting = true;
		OnStartedClosing?.Invoke();

		StartCoroutine(TimedAction(_shiftDuration,
			delta =>
			{
				TimeController.SetTimeScale(this, delta);
				cGroup.alpha = 1f - delta;
			},
			() =>
			{
				IsShifting = false;
				base.Close();
				OnFinishedClosing?.Invoke();
			}));
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

	private bool IsShifting { get; set; }
}
