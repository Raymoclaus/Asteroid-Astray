using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnNoChildren : MonoBehaviour
{
	private Transform t;

	private void Awake()
	{
		t = transform;
	}

	// Update is called once per frame
	void Update()
	{
		if (t.childCount == 0)
		{
			Destroy(gameObject);
		}
	}
}
