using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTimer : MonoBehaviour
{
	public float timer = Mathf.Infinity;

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			Destroy(gameObject);
		}
	}
}
