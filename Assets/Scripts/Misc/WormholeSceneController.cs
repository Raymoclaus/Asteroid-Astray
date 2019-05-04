using System;
using System.Collections;
using UnityEngine;

public class WormholeSceneController : MonoBehaviour
{
	[SerializeField] private CurveTracer shipTracer, shuttleTracer;
	[SerializeField] private float shipTraceSpeed = 0.1f, shuttleTraceSpeed = 0.12f;
	[SerializeField] private CanvasGroup fadeScreen;
	[SerializeField] private DialogueController dialogueCtrl;
	[SerializeField] private Move cameraMover;

	[SerializeField] private float fadeInTime = 5f, fadeOutTime = 10f;
	[SerializeField] private ConversationWithActions
		approachingPlanetDialogue,
		openingWormholeDialogue,
		enteringWormholeDialogue;
	[SerializeField] private float cameraSpeedMultiplier = 3f, cameraSpeedUpTime = 3f;
	[SerializeField] private Material wormhole;
	[SerializeField] private SceneLoader sceneLoader;

	private void Start()
	{
		fadeScreen.alpha = 1f;
		wormhole.SetFloat("_Radius", 0f);

		StartCoroutine(Timer(fadeInTime, (float delta) =>
		{
			fadeScreen.alpha = 1f - delta;
		}, () =>
		{
			shuttleTracer.GoToEndOfPath(shuttleTraceSpeed, resetDistance: true);
			shipTracer.GoToEndOfPath(shipTraceSpeed, resetDistance: true, reachedPathEndAction: () =>
			{
				dialogueCtrl.StartDialogue(approachingPlanetDialogue, false);
			});
		}));
	}

	private void OpenWormhole()
	{
		StartCoroutine(Timer(1f, (float delta) =>
		{
			wormhole.SetFloat("_Radius", delta * 0.35f);
		}, () =>
		{
			ShipEnterWormhole();
		}));
	}

	private void ShipEnterWormhole()
	{
		StartCoroutine(Timer(3f, null, () =>
		{
			shipTracer.GoToEndOfPath(shipTraceSpeed * 10f, reachedPathEndAction: () =>
			{
				DistortWormhole(-1.5f, () => DistortWormhole(-1f));
				ShrinkShip();
				dialogueCtrl.StartDialogue(enteringWormholeDialogue, false);
				shuttleTracer.GoToEndOfPath(shuttleTraceSpeed * 10f);
			});
		}));
	}

	private void DistortWormhole(float distortionAmplitude, Action finishAction = null)
	{
		float currentAmount = wormhole.GetFloat("_DistortionAmplitude");
		StartCoroutine(Timer(1f, (float delta) =>
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
			DistortWormhole(-1.5f, () => DistortWormhole(1f, () => CloseWormhole()));
		});
	}

	private void CloseWormhole()
	{
		StartCoroutine(Timer(1f, (float delta) =>
		{
			wormhole.SetFloat("_Radius", (1f - delta) * 0.35f);
		}, () =>
		{
			StartCoroutine(Timer(fadeOutTime, (float delta) =>
			{
				fadeScreen.alpha = delta;
			}, () => sceneLoader.LoadScene("GameScene")));
		}));
	}

	private void ShrinkShip()
	{
		StartCoroutine(Timer(1f, (float delta) =>
		{
			shipTracer.transform.localScale = Vector3.one * (1f - delta);
		}, null));
	}

	private void ShrinkShuttle()
	{
		StartCoroutine(Timer(1f, (float delta) =>
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
		StartCoroutine(Timer(fadeOutTime, (float delta) =>
		{
			fadeScreen.alpha = delta;
		}, () =>
		{
			dialogueCtrl.StartDialogue(openingWormholeDialogue, false);
		}));
	}

	public void FadeInAndOpenWormhole()
	{
		StartCoroutine(Timer(fadeInTime, (float delta) =>
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
		StartCoroutine(Timer(cameraSpeedUpTime, (float delta) =>
		{
			cameraMover.SetSpeed(Mathf.Lerp(
				originalSpeed, originalSpeed * cameraSpeedMultiplier, delta));
		}, null));
	}

	private IEnumerator Timer(float duration, Action<float> action, Action finishTimerAction)
	{
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			action?.Invoke(Mathf.Clamp01(timer / duration));
			yield return null;
		}
		finishTimerAction?.Invoke();
	}
}
