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

	public static void StartRipple(float rippleWidth = 0.1f, float speed = 2f, float distortionLevel = 0.01f)
	{
		singleton.rippleEffectMat.SetFloat("_RippleWidth", rippleWidth);
		singleton.rippleEffectMat.SetFloat("_DistortionAmplitude", distortionLevel);
		spd = speed;
		time = 0f;
		singleton.StartCoroutine(Ripple());
	}

	private static IEnumerator Ripple()
	{
		while (time < 1f)
		{
			time += Time.deltaTime * spd;
			singleton.rippleEffectMat.SetFloat("_Radius", time);
			yield return null;
		}
	}
}