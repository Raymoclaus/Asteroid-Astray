using CurveTracerSystem;
using CustomYieldInstructions;
using SceneControllers;
using System;
using System.Collections;
using DialogueSystem;
using UnityEngine;
using DialogueSystem.UI;

public class WormholeSceneController : MonoBehaviour
{
	[SerializeField] private CurveTracer shipTracer, shuttleTracer;
	[SerializeField] private float shipTraceSpeed = 0.1f, shuttleTraceSpeed = 0.12f;
	[SerializeField] private CanvasGroup fadeScreen;
	[SerializeField] private Move cameraMover;

	[SerializeField] private float fadeInTime = 5f, fadeOutTime = 10f;
	[SerializeField] private ConversationWithActions
		approachingPlanetDialogue,
		openingWormholeDialogue,
		enteringWormholeDialogue;
	[SerializeField] private float cameraSpeedMultiplier = 3f, cameraSpeedUpTime = 3f;
	[SerializeField] private Material wormhole;

	private void Start()
	{
		fadeScreen.alpha = 1f;
		wormhole.SetFloat("_Radius", 0f);

		StartCoroutine(TimerAction(fadeInTime, (float delta) =>
		{
			fadeScreen.alpha = 1f - delta;
		}, () =>
		{
			shuttleTracer.GoToEndOfPath(shuttleTraceSpeed, resetDistance: true);
			shipTracer.GoToEndOfPath(shipTraceSpeed, resetDistance: true, reachedPathEndAction: () =>
			{
				DialoguePopupUI.ShowDialogue(new DialogueController(approachingPlanetDialogue));
			});
		}));
	}

	private void OpenWormhole()
	{
		StartCoroutine(TimerAction(1f, (float delta) =>
		{
			wormhole.SetFloat("_Radius", delta * 0.35f);
		}, () =>
		{
			ShipEnterWormhole();
		}));
	}

	private void ShipEnterWormhole()
	{
		StartCoroutine(TimerAction(3f, null, () =>
		{
			shipTracer.GoToEndOfPath(shipTraceSpeed * 10f, reachedPathEndAction: () =>
			{
				DistortWormhole(-1.5f, () => DistortWormhole(-1f));
				ShrinkShip();
				DialoguePopupUI.ShowDialogue(new DialogueController(enteringWormholeDialogue));
				shuttleTracer.GoToEndOfPath(shuttleTraceSpeed * 10f);
			});
		}));
	}

	private void DistortWormhole(float distortionAmplitude, Action finishAction = null)
	{
		float currentAmount = wormhole.GetFloat("_DistortionAmplitude");
		StartCoroutine(TimerAction(1f, (float delta) =>
		{
			float amount = Mathf.Lerp(currentAmount, distortionAmplitude, delta);
			wormhole.SetFloat("_DistortionAmplitude", amount);
		}, finishAction));
	}

	public void ShuttleEnterWormhole()
	{
		shuttleTracer.GoToEndOfPath(shuttleTraceSpeed * 10f, reachedPathEndAction: () =>
		{
			ShrinkShuttle();
			DistortWormhole(-1.5f, () => DistortWormhole(1f, () =>
			{
				StartCoroutine(TimerAction(1.5f, null, () => CloseWormhole()));
			}));
		});
	}

	private void CloseWormhole()
	{
		SceneLoader.SceneAsync sa = SceneLoader.PrepareScene("SpaceScene");

		StartCoroutine(TimerAction(1f, (float delta) =>
		{
			wormhole.SetFloat("_Radius", (1f - delta) * 0.35f);
		}, () =>
		{
			StartCoroutine(TimerAction(fadeOutTime / 2f, (float delta) =>
			{
				fadeScreen.alpha = delta;
			}, sa.LoadSceneWhenReady));
		}));
	}

	private void ShrinkShip()
	{
		StartCoroutine(TimerAction(1f, (float delta) =>
		{
			shipTracer.transform.localScale = Vector3.one * (1f - delta);
		}, null));
	}

	private void ShrinkShuttle()
	{
		StartCoroutine(TimerAction(1f, (float delta) =>
		{
			shuttleTracer.transform.localScale = Vector3.one * (1f - delta);
		}, null));
	}

	public void ShipToEndOfPathTwo()
	{
		shipTracer.GoToEndOfPath(shipTraceSpeed, null, true, null);
	}

	public void ShuttleToEndOfPathTwo()
	{
		shuttleTracer.GoToEndOfPath(shuttleTraceSpeed, null, true, null);
	}

	public void FadeOutAndStartOpeningWormholeDialogue()
	{
		StartCoroutine(TimerAction(fadeOutTime, (float delta) =>
		{
			fadeScreen.alpha = delta;
		}, () =>
		{
			DialoguePopupUI.ShowDialogue(new DialogueController(openingWormholeDialogue));
		}));
	}

	public void FadeInAndOpenWormhole()
	{
		StartCoroutine(TimerAction(fadeInTime, (float delta) =>
		{
			fadeScreen.alpha = 1f - delta;
		}, () =>
		{
			OpenWormhole();
		}));
	}

	public void SpeedUp()
	{
		float originalSpeed = cameraMover.GetSpeed();
		StartCoroutine(TimerAction(cameraSpeedUpTime, (float delta) =>
		{
			cameraMover.SetSpeed(Mathf.Lerp(
				originalSpeed, originalSpeed * cameraSpeedMultiplier, delta));
		}, null));
	}

	private Action MethodName = () =>
	{

	};

	private Func<bool> FunctionName = () =>
	{
		return false;
	};

	private IEnumerator TimerAction(float duration, Action<float> action, Action finishTimerAction)
	{
		yield return new ActionOverTime(duration, action);
		finishTimerAction?.Invoke();
	}
}
