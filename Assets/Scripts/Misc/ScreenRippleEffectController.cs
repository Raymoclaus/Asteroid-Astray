using System.Collections;
using UnityEngine;

public class ScreenRippleEffectController : MonoBehaviour
{
	private static ScreenRippleEffectController singleton;
	[SerializeField]
	private Material rippleEffectMat;
	private static float spd = 2f;
	private static float time = 1f;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		//set defaults
		rippleEffectMat.SetFloat("_RippleWidth", 0.1f);
		rippleEffectMat.SetFloat("_DistortionAmplitude", 0.02f);
		rippleEffectMat.SetFloat("_Radius", 3f);
		rippleEffectMat.SetFloat("_PosX", 0.5f);
		rippleEffectMat.SetFloat("_PosY", 0.5f);
	}

	public static void StartRipple(float rippleWidth = 0.1f, float speed = 2f, float distortionLevel = 0.02f,
		Vector2? position = null, float? wait = null)
	{
		Vector2 pos = position ?? Vector2.one * 0.5f;
		singleton.rippleEffectMat.SetFloat("_PosX", pos.x);
		singleton.rippleEffectMat.SetFloat("_PosY", pos.y);
		singleton.rippleEffectMat.SetFloat("_RippleWidth", rippleWidth);
		singleton.rippleEffectMat.SetFloat("_DistortionAmplitude", distortionLevel);
		spd = speed;
		time = 0f;
		singleton.StartCoroutine(Ripple(wait));
	}

	private static IEnumerator Ripple(float? wait = null)
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
			singleton.rippleEffectMat.SetFloat("_Radius", time);
			yield return null;
		}
	}
}