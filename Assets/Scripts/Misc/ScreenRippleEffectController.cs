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
	}

	public static void StartRipple(float rippleWidth = 0.1f, float speed = 2f, float distortionLevel = 0.01f, float? wait = null)
	{
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
				//wait -= 1f / 60f;
				yield return null;
			}
		}
		while (time < 1f)
		{
			time += Time.deltaTime * spd;
			singleton.rippleEffectMat.SetFloat("_Radius", time);
			yield return null;
		}
	}
}