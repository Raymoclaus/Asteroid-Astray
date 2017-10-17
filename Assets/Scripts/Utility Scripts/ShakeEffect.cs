using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShakeEffect {
	public static void Shake(Transform obj, float intensity) {
		obj.localPosition = new Vector2(
			Mathf.Sin(Random.value * Mathf.PI * 2f),
			Mathf.Cos(Random.value * Mathf.PI * 2f))
		* intensity;
	}
}
