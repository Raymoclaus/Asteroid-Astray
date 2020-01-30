using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavyGridController : MonoBehaviour
{
	private const string OFFSET_VAR_NAME = "_WaveOffset";
	[SerializeField] private Material wavyGridMaterial;
	[SerializeField] private bool useUnscaledTime;
	[SerializeField] private float speedMultiplier = 1f;
	[SerializeField] private bool pause;

	private void Update()
	{
		if (pause) return;

		float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		AddOffset(delta * speedMultiplier);
	}

	private void AddOffset(float amount)
	{
		float currentOffset = wavyGridMaterial.GetFloat(OFFSET_VAR_NAME);
		wavyGridMaterial.SetFloat(OFFSET_VAR_NAME, currentOffset + amount);
	}
}
