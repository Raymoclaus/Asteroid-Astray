using System.Collections;
using UnityEngine;

public class SpotlightEffectController : MonoBehaviour
{
	public Material spotlightMaterial;
	private string softRadius = "_SoftRadius", hardRadius = "_Radius";
	private Coroutine currentCoroutine;
	private const float DEFAULT_CLOSED_SOFT_VALUE = 0.2f, DEFAULT_CLOSED_HARD_VALUE = 0.15f;
	private const float DEFAULT_OPEN_SOFT_VALUE = 0.5f, DEFAULT_OPEN_HARD_VALUE = 1.5f;

	public CustomScreenEffect customEffects;

	public void ChangeSpotlight(float hardValue, float softValue, float time, bool ignorePause)
	{
		CancelCoroutine();
		currentCoroutine = Coroutines.MonoObj.StartCoroutine(Go(hardValue, softValue, time, ignorePause));
	}

	public void OpenSpotlightOverTime(float time)
	{
		CancelCoroutine();
		currentCoroutine = Coroutines.MonoObj.StartCoroutine(Go(DEFAULT_OPEN_HARD_VALUE, DEFAULT_OPEN_SOFT_VALUE, time, true));
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

	public void SetSpotlightToOpen()
	{
		SetSpotlight(1.5f, 0.5f, true);
	}

	public void SetSpotlightToClosed()
	{
		SetSpotlight(DEFAULT_CLOSED_HARD_VALUE, DEFAULT_CLOSED_SOFT_VALUE, true);
	}

	public void ActivateSpotlight(bool activate)
	{
		customEffects.SetBlit(spotlightMaterial, activate);
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
			Coroutines.MonoObj.StopCoroutine(currentCoroutine);
		}
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			SetSpotlight(DEFAULT_CLOSED_HARD_VALUE, DEFAULT_CLOSED_SOFT_VALUE);
		}
	}
}
