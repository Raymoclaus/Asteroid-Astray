using System.Collections;
using UnityEngine;

public class SpotlightEffectController : MonoBehaviour
{
	public Material spotlightMaterial;
	private const string SOFT_RADIUS_VAR_NAME = "_SoftRadius",
		HARD_RADIUS_VAR_NAME = "_Radius";
	[SerializeField] private Vector2 closedPreset = new Vector2(0.15f, 0.2f),
		openPreset = new Vector2(1.5f, 0.5f);

	public void ChangeSpotlight(float hardValue, float softValue, float time, bool ignorePause)
	{
		StartCoroutine(Go(hardValue, softValue, time, ignorePause));
	}

	public void OpenSpotlightOverTime(float time)
	{
		StartCoroutine(Go(openPreset.x, openPreset.y, time, true));
	}

	public void SetSpotlight(float hardValue, float softValue, bool cancelCoro = false)
	{
		spotlightMaterial.SetFloat(HARD_RADIUS_VAR_NAME, hardValue);
		spotlightMaterial.SetFloat(SOFT_RADIUS_VAR_NAME, softValue); 
	}

	public void SetSpotlightToOpen()
	{
		SetSpotlight(openPreset.x, openPreset.y, true);
	}

	public void SetSpotlightToClosed()
	{
		SetSpotlight(closedPreset.x, closedPreset.y, true);
	}

	private IEnumerator Go(float hardVal, float softVal, float time, bool ignorePause)
	{
		float timer = 0f;
		float originalSoftVal = spotlightMaterial.GetFloat(SOFT_RADIUS_VAR_NAME);
		float originalHardVal = spotlightMaterial.GetFloat(HARD_RADIUS_VAR_NAME);
		while (timer < time)
		{
			timer += ignorePause ? Time.unscaledDeltaTime : Time.deltaTime;

			float currentHardVal = Mathf.Lerp(originalHardVal, hardVal, timer / time);
			float currentSoftVal = Mathf.Lerp(originalSoftVal, softVal, timer / time);
			SetSpotlight(currentHardVal, currentSoftVal, false);
			yield return null;
		}
	}
}
