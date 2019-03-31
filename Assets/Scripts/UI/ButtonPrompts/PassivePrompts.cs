using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassivePrompts : MonoBehaviour
{
	private ShuttleTrackers shuttleTrackers;
	private ShuttleTrackers Trackers
	{
		get
		{
			return shuttleTrackers ?? (shuttleTrackers = Resources.Load<ShuttleTrackers>("ShuttleTrackerSO"));
		}
	}
	private Character mainChar;
	private Character MainChar
	{
		get
		{
			return mainChar ?? (mainChar = FindObjectOfType<Shuttle>());
		}
	}

	[SerializeField] private GameObject
		goPrompt,
		shootPrompt,
		boostPrompt,
		launchPrompt,
		pausePrompt;

	private void Update()
	{
		SetActive(shootPrompt, Trackers.canShoot);
		SetActive(boostPrompt, Trackers.canBoost);
		SetActive(launchPrompt, (MainChar?.IsDrilling ?? false) && Trackers.canLaunch);
	}

	private void SetActive(GameObject obj, bool active)
	{
		if (active == obj.activeSelf) return;

		obj.SetActive(active);
	}
}
