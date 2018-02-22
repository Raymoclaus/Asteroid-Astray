using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleReactToThrusters : MonoBehaviour
{
	public ParticleSystem ps;
	//for getting its rotation and apply velocity based on rotation
	public ThrusterController thruster;
	List<ParticleSystem.Particle> parts = new List<ParticleSystem.Particle>();
	public float thrusterResistance = 0.1f;

	private void Awake()
	{
		ps = ps == null ? GetComponent<ParticleSystem>() : ps;
	}

	private void OnParticleTrigger()
	{
		int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);

		Vector3 velocity = thruster.ThrusterDirection * thrusterResistance;

		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = parts[i];
			p.velocity += velocity;
			parts[i] = p;
		}

		ps.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, parts);
	}
}