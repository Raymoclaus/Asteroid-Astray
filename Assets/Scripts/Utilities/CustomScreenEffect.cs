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
		if (enabled)
		{
			noBlit = new bool[effects.Length];
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
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

		rts.Clear();

		for (int i = 0; i < effectsToBlit.Count; i++)
		{
			rts.Add(i == 0 ? new RenderTexture(Screen.width, Screen.height, 0) : new RenderTexture(rts[i - 1]));
			rts[i].filterMode = FilterMode.Point;
			Graphics.Blit(source, rts[i], effectsToBlit[i]);
			source = rts[i];
		}
		Graphics.Blit(source, destination);
		
		foreach (RenderTexture rt in rts)
		{
			rt.Release();
		}
	}

	public void SetNoBlit(int index, bool shouldNotBlit)
	{
		noBlit[index] = shouldNotBlit;
		if (!shouldNotBlit) enabled = true;
	}
}