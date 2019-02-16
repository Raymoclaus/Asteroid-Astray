using System.Collections;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spotlight Effect Controller")]
public class SpotlightEffectController : ScriptableObject
{
	[SerializeField] private Material spotlightMaterial;
	private string softRadius = "_SoftRadius", hardRadius = "_Radius";
	private Coroutine currentCoroutine;
	private const float DEFAULT_SOFT_VALUE = 0.2f, DEFAULT_HARD_VALUE = 0.15f;

	public void ChangeSpotlight(float hardValue, float softValue, float time, bool ignorePause)
	{
		CancelCoroutine();
		currentCoroutine = Coroutines.Instance.StartCoroutine(Go(hardValue, softValue, time, ignorePause));
	}

	public void ChangeSpotlight(float time)
	{
		CancelCoroutine();
		currentCoroutine = Coroutines.Instance.StartCoroutine(Go(1.5f, 0.5f, time, true));
	}

	public void SetSpotlight(float hardValue, float softValue, bool cancelCoro = false)
	{
		if (cancelCoro)
		{
			CancelCoroutine();
		}
		spotlightMaterial.SetFloat(hardRadius, hardValue);
		spotlightMaterial.SetFloat(softRadius, softValue); 
	}

	public void SetSpotlight()
	{
		SetSpotlight(1.5f, 0.5f, true);
	}

	private IEnumerator Go(float hardVal, float softVal, float time, bool ignorePause)
	{
		float timer = 0f;
		float originalSoftVal = spotlightMaterial.GetFloat(softRadius);
		float originalHardVal = spotlightMaterial.GetFloat(hardRadius);
		while (timer < time)
		{
			timer += ignorePause ? Time.unscaledDeltaTime : Time.deltaTime;

			float currentHardVal = Mathf.Lerp(originalHardVal, hardVal, timer / time);
			float currentSoftVal = Mathf.Lerp(originalSoftVal, softVal, timer / time);
			SetSpotlight(currentHardVal, currentSoftVal, false);
			yield return null;
		}
	}

	private void CancelCoroutine()
	{
		if (currentCoroutine != null)
		{
			Coroutines.Instance.StopCoroutine(currentCoroutine);
		}
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			SetSpotlight(DEFAULT_HARD_VALUE, DEFAULT_SOFT_VALUE);
		}
	}
}
