using UnityEngine;

public class PassivePrompts : MonoBehaviour
{
	private Shuttle mainChar;
	private Shuttle MainChar => mainChar != null ? mainChar
		: (mainChar = FindObjectOfType<Shuttle>());

	[SerializeField] private GameObject
		goPrompt,
		shootPrompt,
		boostPrompt,
		launchPrompt,
		pausePrompt;

	private void Update()
	{
		SetActive(shootPrompt, MainChar?.CanShoot ?? false);
		SetActive(boostPrompt, MainChar?.CanBoost ?? false);
		SetActive(launchPrompt,
			MainChar == null ? false : MainChar.IsDrilling && MainChar.CanLaunch);
	}

	private void SetActive(GameObject obj, bool active)
	{
		if (active == obj.activeSelf) return;
		obj.SetActive(active);
	}
}
