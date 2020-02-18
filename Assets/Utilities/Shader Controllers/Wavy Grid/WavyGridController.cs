using UnityEngine;

public class WavyGridController : MonoBehaviour
{
	[SerializeField] private Material wavyGridMaterial;
	[SerializeField] private bool useUnscaledTime;
	[Header("Wave Variables")]
	[SerializeField] private float waveSpeed = 1f;
	[SerializeField] private bool pauseWave;
	private const string OFFSET_VAR_NAME = "_WaveOffset";
	[Header("Vignette Variables")]
	[SerializeField] private Vector2 emptyVignettePreset = new Vector2(1f, 0f);
	[SerializeField] private Vector2 unpausedVignettePreset = new Vector2(0.6f, 0.3f);
	[SerializeField] private Vector2 pausedVignettePreset = new Vector2(0f, 0.3f);
	private float transitionTimer = 0f;
	[SerializeField] private float transitionSpeed = 1f;
	private Vector2? priorVignette, targetVignette;
	[SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	private const string VIGNETTE_SIZE_VAR_NAME = "_VignetteSize";
	private const string VIGNETTE_WIDTH_VAR_NAME = "_VignetteWidth";

	private void Awake()
	{
		priorVignette = priorVignette ?? unpausedVignettePreset;
		targetVignette = targetVignette ?? priorVignette;

		Pause.OnPause += TransitionVignetteToPausedState;
		Pause.OnResume += TransitionVignetteToUnpausedState;
	}

	private void OnDestroy()
	{
		Pause.OnPause -= TransitionVignetteToPausedState;
		Pause.OnResume -= TransitionVignetteToUnpausedState;
	}

	private void Update()
	{
		float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

		if (!pauseWave)
		{
			AddOffset(delta * waveSpeed);
		}

		transitionTimer += delta * transitionSpeed;
		float transitionCurveEvaluation = transitionCurve.Evaluate(transitionTimer);
		Vector2 lerpedTransitionValue =
			Vector2.Lerp((Vector2) priorVignette, (Vector2) targetVignette, transitionCurveEvaluation);
		SetVignetteState(lerpedTransitionValue);
	}

	private Vector2 CurrentVignetteState
		=> new Vector2(
			wavyGridMaterial.GetFloat(VIGNETTE_SIZE_VAR_NAME),
			wavyGridMaterial.GetFloat(VIGNETTE_WIDTH_VAR_NAME));

	private void AddOffset(float amount)
	{
		float currentOffset = wavyGridMaterial.GetFloat(OFFSET_VAR_NAME);
		wavyGridMaterial.SetFloat(OFFSET_VAR_NAME, currentOffset + amount);
	}

	public void TransitionVignette(Vector2 target, float? speed = null)
	{
		transitionTimer = 0f;
		targetVignette = target;
		transitionSpeed = speed ?? transitionSpeed;
		priorVignette = CurrentVignetteState;
	}

	public void TransitionVignetteToPausedState() => TransitionVignette(pausedVignettePreset);

	public void TransitionVignetteToUnpausedState() => TransitionVignette(unpausedVignettePreset);

	public void SetVignetteState(Vector2 target)
	{
		wavyGridMaterial.SetFloat(VIGNETTE_SIZE_VAR_NAME, target.x);
		wavyGridMaterial.SetFloat(VIGNETTE_WIDTH_VAR_NAME, target.y);
	}
}
