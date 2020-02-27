using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UICanvasHider : MonoBehaviour
{
	private CanvasGroup cg;
	private CanvasGroup CGroup => cg != null ? cg : (cg = GetComponent<CanvasGroup>());
	private float start, target, delta;
	[SerializeField] private AnimationCurve curve;
	[SerializeField] private float transitionDuration;

	private void Update()
	{
		delta += Time.deltaTime * (1f / transitionDuration);
		float evaluation = curve.Evaluate(delta);
		float lerpValue = Mathf.Lerp(start, target, evaluation);
		SetCurrentAlpha(lerpValue);
	}

	public void SetTarget(float target)
	{
		this.target = target;
		start = CGroup.alpha;
		delta = 0f;
	}

	public void Show()
	{
		SetTarget(1f);
	}

	public void Hide()
	{
		SetTarget(0f);
	}

	public void InstantSetAlpha(float alpha)
	{
		delta = 1f;
		target = alpha;
		start = alpha;
		SetCurrentAlpha(alpha);
	}

	public void SetCurrentAlpha(float alpha)
	{
		CGroup.alpha = alpha;
	}
}
