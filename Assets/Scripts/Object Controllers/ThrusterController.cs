using System.Collections;
using UnityEngine;

public class ThrusterController : MonoBehaviour
{
	public ParticleSystem[] systems;
	public Shuttle shuttle;
	public float baseSpeed = 1f;
	public float baseTrailWidth = 0.04f;
	public float speedMod = 1f;

	private void Update()
	{
		SetSystemsValues();
	}

	private void SetSystemsValues()
	{
		float shuttleMag = shuttle._vel.magnitude;
		float speed = baseSpeed * shuttleMag * speedMod;
		float trailWidth = baseTrailWidth * speed / baseSpeed;

		foreach (ParticleSystem ps in systems)
		{
			ParticleSystem.VelocityOverLifetimeModule volMod = ps.velocityOverLifetime;
			volMod.speedModifierMultiplier = speed;

			ParticleSystem.TrailModule trailMod = ps.trails;
			trailMod.widthOverTrail = trailWidth;
		}
	}
}