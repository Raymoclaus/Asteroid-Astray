using UnityEngine;
using UnityEngine.Events;

public class FadeComponent : MonoBehaviour
{
	[SerializeField] [TextArea] private string description;
	[SerializeField] private FadeScreen fadeCanvas;
	[SerializeField] private float duration = 1.5f;
	[SerializeField] private Color fadeColor;
	[SerializeField] private bool fadeInOnStart = false;
	private bool reachedHalfway = false;

	public UnityEvent OnFadeStarted, OnFadeEnded, OnFadeHalfway;

	private void Start()
	{
		if (!fadeInOnStart) return;
		StartFadeIn();
	}

	public void StartFadeIn()
	{
		OnFadeStarted?.Invoke();
		fadeCanvas.SetColour(fadeColor);
		fadeCanvas.FadeIn(duration,
			finishedAction: OnFadeEnded.Invoke,
			updatedAction: FadeUpdated);
	}

	public void StartFadeOut()
	{
		OnFadeStarted?.Invoke();
		fadeCanvas.SetColour(fadeColor);
		fadeCanvas.FadeOut(duration,
			finishedAction: OnFadeEnded.Invoke,
			updatedAction: FadeUpdated);
	}

	private void FadeUpdated(float delta)
	{
		if (reachedHalfway) return;

		if (delta < 0.5f) return;
		reachedHalfway = true;
		OnFadeHalfway?.Invoke();
	}
}
