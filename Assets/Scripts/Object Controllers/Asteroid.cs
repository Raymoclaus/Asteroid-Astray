using UnityEngine;

public class Asteroid : Entity, IDrillableObject
{

	#region Fields
	[Header("Asteroid Fields")]
	[Tooltip("List of available sprites to choose from.")]
	public Sprite[] Shapes;
	[Tooltip("Reference to the sprite renderer of the asteroid.")]
	public SpriteRenderer SprRend;
	[Tooltip("Reference to the asteroid destruction particle effect prefab.")]
	public Transform destructionParticleEffect;
	//for testing resource drop
	public ResourceDrop resource;
	[Tooltip("Reference to the shake effect script on the sprite.")]
	public ShakeEffect ShakeFX;
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float SpinSpeedRange;
	[Tooltip("Picks a random value between given value and negative given value to determine starting velocity")]
	public float VelocityRange;
	[Tooltip("Upper limit for health stat.")]
	public float MaxHealth = 150f;
	//current health value between 0 and MaxHealth
	private float Health;
	#endregion

	#region Audio
	public AudioClip[] shatterSounds;
	public Vector2 shatterPitchRange;
	#endregion

	public override void Awake()
	{
		base.Awake();
		//pick a random sprite from given list of sprites
		SprRend.sprite = Shapes[Random.Range(0, Shapes.Length)];
		RandomMovement();
		//start health at max value
		Health = MaxHealth;

		initialised = true;
	}

	private void RandomMovement()
	{
		//picks a random speed to spin at within a given range with chance favoring lower values
		Rb.AddTorque((Mathf.Pow(Random.Range(0f, 2f), 2f) * SpinSpeedRange - SpinSpeedRange));
		//picks a random direction and velocity within a given range with chance favoring lower values
		Rb.velocity = new Vector2(
			Mathf.Sin(Random.value * 2f * Mathf.PI),
			Mathf.Cos(Random.value * 2f * Mathf.PI))
			* (Mathf.Pow(Random.Range(0f, 2f), 2f) * VelocityRange - VelocityRange);
	}

	private void DestroySelf(bool explode)
	{
		if (explode)
		{
			//particle effect
			if (destructionParticleEffect != null)
			{
				Transform fx = Instantiate(destructionParticleEffect);
				fx.position = transform.position;
			}

			//sound effect
			AudioManager.PlaySFX(shatterSounds[Random.Range(0, shatterSounds.Length)], transform.position, pitch: Random.Range(shatterPitchRange.x, shatterPitchRange.y));

			//drop resources
			for (int i = 0; i < Random.Range(1, 4); i++)
			{
				Transform drop = Instantiate(resource).transform;
				drop.position = transform.position;
			}
		}

		base.DestroySelf();
	}

	// If health is below zero, this will destroy itself
	private bool CheckHealth()
	{
		if (Health > 0f) return false;
		DestroySelf(true);
		return true;
	}

	//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
	public bool TakeDrillDamage(float damage)
	{
		//take damage
		Health -= damage;
		//calculate shake intensity. Gets more intense the less health it has
		ShakeFX.SetIntensity(damage / MaxHealth * (3f - (Health / MaxHealth * 2f)));
		return CheckHealth();
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		ShakeFX.Begin();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill)
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();

			if ((Entity)otherDrill.drillTarget == this)
			{
				otherDrill.StopDrilling();
			}
		}
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		ShakeFX.Stop();
	}

	//If queried, this object will say that it is an asteroid-type Entity
	public override EntityType GetEntityType()
	{
		return EntityType.Asteroid;
	}

	public override void PhysicsReEnabled()
	{
		RandomMovement();
	}
}