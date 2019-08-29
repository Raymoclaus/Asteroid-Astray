using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetInteractablePrompt : InteractablePromptTrigger
{
	private int exploredCount = 0;
	private int seed;

	protected override void Awake()
	{
		base.Awake();

		Vector2 originalPos = transform.position;
		float distance = originalPos.magnitude;
		float modifiedDistance = distance / BgCameraController.SCROLL_SPEED - distance;
		Vector2 modifiedPos = originalPos.normalized * modifiedDistance;
		SetColliderOffset(modifiedPos);
	}

	public void SetSeed(int seed)
	{
		this.seed = seed;
		Random.InitState(seed);
	}

	protected override void OnInteracted(Triggerer actor)
	{
		base.OnInteracted(actor);
		GoToPlanet();
	}

	private void GoToPlanet()
	{
		SceneLoader.LoadSceneStatic("PlanetScene");
	}
}
