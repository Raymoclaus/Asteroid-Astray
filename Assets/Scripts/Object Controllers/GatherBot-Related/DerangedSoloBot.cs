using System.Collections;
using DialogueSystem;
using DialogueSystem.UI;
using UnityEngine;

public class DerangedSoloBot : SoloBot
{
	private Vector2 randomWanderTarget;
	[SerializeField] private float wanderDistance = 3f;
	[SerializeField] private ParticleSystem sparks;
	[SerializeField] private float sparkChance = 0.02f;
	[SerializeField] private float sparkKickBackStrength = 1f;
	[SerializeField] private float sparkMinimumDelay = 1f;
	[SerializeField] private ConversationWithActions destroyingFirstDerangedBot;
	private float lastSparkTime;

	protected override void OnSpawn()
	{
		SetState(AIState.Wandering);
		Activate(true);
		hive = null;
		ChooseRandomWanderTarget();
	}

	protected override void SetState(AIState newState)
	{
		switch (newState)
		{
			default:
				if (newState == AIState.Dying)
				{
					SendPassiveDialogue(destroyingFirstDerangedBot, true);
				}
				base.SetState(newState);
				break;
			case AIState.Spawning:
			case AIState.Scanning:
			case AIState.Gathering:
			case AIState.Exploring:
			case AIState.Storing:
			case AIState.Suspicious:
			case AIState.Signalling:
			case AIState.Attacking:
			case AIState.Collecting:
			case AIState.Escaping:
			case AIState.Wandering:
				base.SetState(AIState.Wandering);
				break;
		}
	}

	protected override void Wandering()
	{
		if (GoToLocation(randomWanderTarget, true, 1f, true, null))
		{
			ChooseRandomWanderTarget();
		}

		float randomVal = Random.value;
		if (randomVal <= sparkChance)
		{
			Spark();
		}
	}

	private void Spark()
	{
		if (Time.time < lastSparkTime + sparkMinimumDelay) return;

		lastSparkTime = Time.time;
		float randomAngle = Random.value * Mathf.PI * 2f;
		Vector2 randomDir = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
		Vector2 force = randomDir * sparkKickBackStrength;
		rb.velocity += force;

		Vector2 sparkDirection = -randomDir;
		float angle = Vector2.SignedAngle(Vector2.up, sparkDirection);
		sparks.transform.position = transform.position;
		sparks.transform.eulerAngles = Vector3.forward * angle;
		sparks.Play();
	}

	private void ChooseRandomWanderTarget()
	{
		float randomAngle = Random.value * Mathf.PI * 2f;
		Vector2 randomDir = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
		randomWanderTarget = (Vector2)transform.position + randomDir * wanderDistance;
	}

	protected override IEnumerator ChargeForcePulse() { yield break; }
}
