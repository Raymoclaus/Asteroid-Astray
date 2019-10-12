using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorReplacementGroup : MonoBehaviour
{
	public List<ColorReplacement> colorReplacementComponents;

	public void SetColor(Color col)
	{
		for (int i = 0; i < colorReplacementComponents.Count; i++)
		{
			colorReplacementComponents[i].SetColor(col);
		}
	}

	public void SetBlendAmount(float blendAmount)
	{
		for (int i = 0; i < colorReplacementComponents.Count; i++)
		{
			colorReplacementComponents[i].SetBlendAmount(blendAmount);
		}
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
