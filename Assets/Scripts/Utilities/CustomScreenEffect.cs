using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CustomScreenEffect : MonoBehaviour
{
	public Material[] effects;
	[SerializeField]
	private bool[] noBlit;
	public Camera cam;
	private List<Material> effectsToBlit = new List<Material>();
	private List<RenderTexture> rts = new List<RenderTexture>();

	private void Awake()
	{
		enabled = effects.Length > 0;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CheckRTSCount();
		effectsToBlit.Clear();
		
		for (int i = 0; i < effects.Length; i++)
		{
			if (noBlit[i] || effects[i] == null) continue;
			effectsToBlit.Add(effects[i]);
		}

		if (effectsToBlit.Count == 0)
		{
			enabled = false;
			Graphics.Blit(source, destination);
			return;
		}

		if (effectsToBlit.Count == 1)
		{
			Graphics.Blit(source, destination, effectsToBlit[0]);
			return;
		}

		if (effectsToBlit.Count > 1)
		{
			for (int i = 0; i < effectsToBlit.Count; i++)
			{
				Graphics.Blit(source, rts[i], effectsToBlit[i]);
				source = rts[i];
			}
			Graphics.Blit(source, destination);
		}

		RenderTexture currentRT = RenderTexture.active;
		foreach (RenderTexture rt in rts)
		{
			RenderTexture.active = rt;
			GL.Clear(false, true, Color.clear);
			rt.DiscardContents();
			rt.Release();
		}
		RenderTexture.active = currentRT;
	}

	public void SetNoBlit(int index, bool shouldNotBlit)
	{
		noBlit[index] = shouldNotBlit;
		if (!shouldNotBlit) enabled = true;
	}

	private void CheckRTSCount()
	{
		if (rts.Count >= effects.Length) return;

		for (int i = rts.Count; i < effects.Length; i++)
		{
			rts.Add(new RenderTexture(Screen.width, Screen.height, 0));
		}
	}
}