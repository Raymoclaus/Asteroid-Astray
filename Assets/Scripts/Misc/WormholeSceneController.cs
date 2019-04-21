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
	[SerializeField] private ConversationEvent approachingPlanetDialogue;
	[SerializeField] private float cameraSpeedMultiplier = 3f, cameraSpeedUpTime = 3f;

	private void Start()
	{
		fadeScreen.alpha = 1f;

		StartCoroutine(Timer(fadeInTime, (float delta) =>
		{
			fadeScreen.alpha = 1f - delta;
		}, () =>
		{
			shipTracer.GoToEndOfPath(shipTraceSpeed, resetDistance: true, reachedPathEndAction: () =>
			{
				dialogueCtrl.StartDialogue(approachingPlanetDialogue, false);
			});
			shuttleTracer.GoToEndOfPath(shuttleTraceSpeed, resetDistance: true);

			approachingPlanetDialogue.conversationEndAction.AddListener(() =>
			{
				float originalSpeed = cameraMover.speed;
				StartCoroutine(Timer(cameraSpeedUpTime, (float delta) =>
				{
					cameraMover.speed = Mathf.Lerp(originalSpeed, originalSpeed * cameraSpeedMultiplier, delta);
				}, null));
				shipTracer.GoToEndOfPath(shipTraceSpeed, null, true, null);
				shuttleTracer.GoToEndOfPath(shuttleTraceSpeed, null, true, null);
				StartCoroutine(Timer(fadeOutTime, (float delta) =>
				{
					fadeScreen.alpha = delta;
				}, null));
			});
		}));
	}

	private IEnumerator Timer(float duration, Action<float> action, Action finishTimerAction)
	{
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			action(Mathf.Clamp01(timer / duration));
			yield return null;
		}
		finishTimerAction?.Invoke();
	}
}
