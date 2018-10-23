﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LaunchTrailController : MonoBehaviour
{
	[SerializeField]
	private Animator anim;
	private Transform followTarget;
	private float angle;

	private void Awake()
	{
		anim = anim ?? GetComponent<Animator>();
		transform.parent = ParticleGenerator.holder;
	}

	private void Update()
	{
		if (followTarget == null) return;
		transform.position = followTarget.position;
	}

	public void SetFollowTarget(Transform target, Vector2 direction, float scale = 1f)
	{
		followTarget = target;
		transform.eulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.up, -direction);
		transform.localScale = Vector3.one * scale;
	}

	public void EndLaunchTrail()
	{
		anim.SetTrigger("End");
	}

	public void CutLaunchTrail()
	{
		Destroy(gameObject);
	}
}
