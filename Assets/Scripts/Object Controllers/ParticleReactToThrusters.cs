using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleReactToThrusters : MonoBehaviour
{
	public ParticleSystem ps;
	//for getting its rotation and apply velocity based on rotation
	public ThrusterController thruster;
	public Component thrusterRef;
	List<ParticleSystem.Particle> parts = new List<ParticleSystem.Particle>();
	public float thrusterResistance = 0.1f;

	private void Awake()
	{
		ps = ps ?? GetComponent<ParticleSystem>();
		thruster = thruster ?? FindObjectOfType<ThrusterController>();
	}

	private void Update()
	{
		if (thrusterRef == null)
		{
			thrusterRef = thrusterRef == null ? thruster.thrusterCol : thrusterRef;
			ps.trigger.SetCollider(0, thrusterRef);
		}
	}

	private void OnParticleTrigger()
	{
		int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);
		if (numEnter == 0) return;

		Vector3 velocity = Vector3.zero;
		if (thruster != null)
		{
			velocity = thruster.ThrusterDirection* thrusterResistance;
		}

		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = parts[i];
			p.velocity += velocity;
			parts[i] = p;
		}

		ps.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);
	}
}