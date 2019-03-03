using UnityEngine;

public class ThrusterController : MonoBehaviour
{
	public ParticleSystem[] thrusterFire;
	public ParticleSystem[] smokeTrails;
	public Shuttle shuttle;
	public float baseSpeed = 1f;
	public float baseTrailWidth = 0.04f;
	public float speedMod = 0.3f;
	public Transform thrusterForceHolder;
	public AreaEffector2D thrusterArea;
	public Component thrusterCol;
	public float thrusterStrengthMod = 0.5f;
	private float shuttleMag;
	public Vector3 ThrusterDirection
	{
		get
		{
			return -new Vector3(Mathf.Sin(Mathf.Deg2Rad * -shuttle.rot.z), Mathf.Cos(Mathf.Deg2Rad * -shuttle.rot.z), 0f) * thrusterStrengthMod * shuttleMag;
		}
	}

	private void Awake()
	{
		//get required references
		shuttle = shuttle == null ? GetComponentInParent<Shuttle>() : shuttle;
		thrusterArea = thrusterArea == null ? GetComponentInChildren<AreaEffector2D>() : thrusterArea;
		thrusterCol = thrusterCol == null ? thrusterArea.GetComponent<Component>() : thrusterCol;
	}

	private void Update()
	{
		if (!Pause.IsPaused)
		{
			shuttleMag = shuttle.velocity.magnitude;
			SetThrusterFireValues();
			SetThrusterForceValues();
			SetSmokeTrailState();
		}
	}

	private void SetThrusterFireValues()
	{
		float speed = baseSpeed * shuttleMag * speedMod;
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
		thrusterForceHolder.localScale = Vector3.one * shuttleMag * speedMod;
		if (thrusterArea != null)
		{
			thrusterArea.forceMagnitude = shuttleMag * thrusterStrengthMod;
		}
	}

	private void SetSmokeTrailState()
	{
		bool active = shuttle.accel != Vector2.zero;

		for (int i = 0; i < smokeTrails.Length; i++)
		{
			ParticleSystem ps = smokeTrails[i];
			if (active)
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