using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ScreenRippleEffectController")]
public class ScreenRippleEffectController : ScriptableObject
{
	[SerializeField]
	private Material rippleEffectMat;
	private static float spd = 2f;
	private static float time = 1f;

	private void OnDisable()
	{
		//set defaults
		rippleEffectMat.SetFloat("_RippleWidth", 0.1f);
		rippleEffectMat.SetFloat("_DistortionAmplitude", 0.02f);
		rippleEffectMat.SetFloat("_Radius", 3f);
		rippleEffectMat.SetFloat("_PosX", 0.5f);
		rippleEffectMat.SetFloat("_PosY", 0.5f);
	}

	public void StartRipple(MonoBehaviour mono, float rippleWidth = 0.1f, float speed = 2f, float distortionLevel = 0.02f,
		Vector2? position = null, float? wait = null)
	{
		Vector2 pos = position ?? Vector2.one * 0.5f;
		rippleEffectMat.SetFloat("_PosX", pos.x);
		rippleEffectMat.SetFloat("_PosY", pos.y);
		rippleEffectMat.SetFloat("_RippleWidth", rippleWidth);
		rippleEffectMat.SetFloat("_DistortionAmplitude", distortionLevel);
		spd = speed;
		time = 0f;
		mono.StartCoroutine(Ripple(wait));
	}

	private IEnumerator Ripple(float? wait = null)
	{
		if (wait != null)
		{
			while (wait > 0f)
			{
				wait -= Time.unscaledDeltaTime;
				yield return null;
			}
		}
		while (time < 3f)
		{
			time += Time.deltaTime * spd;
			rippleEffectMat.SetFloat("_Radius", time);
			yield return null;
		}
	}
}