using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleReactToThrusters : MonoBehaviour
{
	[SerializeField] private ParticleSystem ps;
	private List<ParticleSystem.Particle> parts = new List<ParticleSystem.Particle>();
	[SerializeField] private float thrusterResistance = 0.1f;
	//this is static because OnParticleTrigger cannot tell which collider was triggered, so I will only let one thing affect particles
	private static ThrusterController _thrusterController;
	private static event Action OnThrusterControllerChanged;

	private void OnEnable()
	{
		if (_thrusterController != null)
		{
			UpdateTrigger();
		}
		OnThrusterControllerChanged += UpdateTrigger;
	}

	private void OnDisable()
	{
		OnThrusterControllerChanged -= UpdateTrigger;
	}

	private void OnParticleTrigger()
	{
		int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);
		if (numEnter == 0) return;

		Vector3 velocity = Vector3.zero;
		if (_thrusterController != null)
		{
			velocity = _thrusterController.ThrusterDirection * thrusterResistance;
		}

		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = parts[i];
			p.velocity += velocity;
			parts[i] = p;
		}

		ps.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);
	}

	private void UpdateTrigger()
	{
		ps.trigger.SetCollider(0, _thrusterController.ThrusterCollider);
	}

	public static void SetThrusterController(ThrusterController tc)
	{
		_thrusterController = tc;
		OnThrusterControllerChanged?.Invoke();
	}
}