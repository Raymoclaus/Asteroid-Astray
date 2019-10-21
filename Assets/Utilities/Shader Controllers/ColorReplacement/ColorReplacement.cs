using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorReplacement : MonoBehaviour
{
	public Renderer rend;
	MaterialPropertyBlock mpb;

	private void Awake()
	{
		mpb = new MaterialPropertyBlock();
		rend = rend ?? GetComponent<Renderer>();
	}

	public void SetColor(Color col)
	{
		rend.GetPropertyBlock(mpb);
		mpb.SetColor("_Color", col);
		rend.SetPropertyBlock(mpb);
	}

	public void SetBlendAmount(float blendAmount)
	{
		rend.GetPropertyBlock(mpb);
		mpb.SetFloat("_BlendAmount", blendAmount);
		rend.SetPropertyBlock(mpb);
	}

	public void Flash(float time = 0.5f, Color? col = null)
	{
		if (col != null)
		{
			SetColor((Color)col);
		}
		StartCoroutine(FlashCoro(time));
	}

	private IEnumerator FlashCoro(float time)
	{
		bool flashOn = true;
		while (time > 0f)
		{
			bool wasEven = (int)(time / 0.1f) % 2 == 0;
			time -= Time.deltaTime;
			bool isEven = (int)(time / 0.1f) % 2 == 0;
			if (isEven != wasEven)
			{
				flashOn = !flashOn;
				SetBlendAmount(flashOn ? 1f : 0f);
			}
			yield return null;

		}

		SetBlendAmount(0f);
	}
}