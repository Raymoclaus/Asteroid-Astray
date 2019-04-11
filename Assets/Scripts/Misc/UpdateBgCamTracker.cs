using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateBgCamTracker : MonoBehaviour
{
	[SerializeField] private BgCamTracker tracker;

	private void Update()
	{
		tracker.position = transform.position;
	}
}
