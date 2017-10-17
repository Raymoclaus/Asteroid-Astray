using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
	public Transform target;

	void Start()
	{
		//if there is no target then create a default one
		if (target == null)
		{
			target = new GameObject("camTarget").transform;
			Debug.Log("Default camera target created");
		}
	}

	void Update()
	{
		//stay above target
		FollowTarget();
	}

	//sets position to be just above the target
	private void FollowTarget()
	{
		transform.position = target.position + target.forward * -10f;
	}
}
