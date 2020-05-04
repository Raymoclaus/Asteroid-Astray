using UnityEngine;

public class ThrusterController : MonoBehaviour
{
	[SerializeField] private ParticleSystem[] thrusterFire;
	[SerializeField] private ParticleSystem[] smokeTrails;
	[SerializeField] private float baseSpeed = 1f;
	[SerializeField] private float baseTrailWidth = 0.04f;
	[SerializeField] private float speedMod = 0.3f;
	[SerializeField] private Transform thrusterForceHolder;
	[SerializeField] private AreaEffector2D ThrusterAreaEffector;
	[SerializeField] private Collider2D _thrusterCollider;
	[SerializeField] private float thrusterStrengthMod = 0.5f;

	private void Awake()
	{
		ParticleReactToThrusters.SetThrusterController(this);
	}

	private void Update()
	{
		if (TimeController.IsStopped) return;

		SetThrusterFireValues();
		SetThrusterForceValues();
		SetSmokeTrailState();
	}

	public float Speed { get; set; }

	public bool IsAccelerating { get; set; }

	public float ZRotation { get; set; }

	public Collider2D ThrusterCollider => _thrusterCollider;

	public Vector3 ThrusterDirection
		=> -new Vector3(Mathf.Sin(Mathf.Deg2Rad * -ZRotation),
			   Mathf.Cos(Mathf.Deg2Rad * -ZRotation),
			   0f)
		   * thrusterStrengthMod * Speed;

	private void SetThrusterFireValues()
	{
		float speed = baseSpeed * Speed * speedMod;
		float trailWidth = baseTrailWidth * speed / baseSpeed;

		for (int i = 0; i < thrusterFire.Length; i++)
		{
			ParticleSystem ps = thrusterFire[i];
			ParticleSystem.VelocityOverLifetimeModule volMod = ps.velocityOverLifetime;
			volMod.speedModifierMultiplier = speed;

			ParticleSystem.TrailModule trailMod = ps.trails;
			trailMod.widthOverTrail = trailWidth;
		}
	}

	private void SetThrusterForceValues()
	{
		thrusterForceHolder.localScale = Vector3.one * Speed * speedMod;
		if (ThrusterAreaEffector != null)
		{
			ThrusterAreaEffector.forceMagnitude = Speed * thrusterStrengthMod;
		}
	}

	private void SetSmokeTrailState()
	{
		for (int i = 0; i < smokeTrails.Length; i++)
		{
			ParticleSystem ps = smokeTrails[i];
			if (IsAccelerating)
			{
				if (!ps.isEmitting)
				{
					ps.Play();
				}
			}
			else
			{
				if (ps.isEmitting)
				{
					ps.Stop();
				}
			}
		}
	}
}