using UnityEngine;

public class Asteroid : Entity, IDrillableObject, IDamageable
{
	[System.Serializable]
	private struct ColliderInfo
	{
		public int type;
		public Vector2 size;
		public Vector2 offset;
		public float rotation;
	}

	#region Fields
	[Header("Asteroid Fields")]
	[Tooltip("Reference to the sprite renderer of the asteroid.")]
	public SpriteRenderer SprRend;
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
	//reference to all the sprites
	public LoadedResources loadRes;
	//collection of info about the colliders the large asteroids should use
	[SerializeField]
	private ColliderInfo[] largeInfo;
	//chance that an asteroid will be larger than normal
	[SerializeField]
	private float largeChance = 0.01f;
	//keeps track of size type and ID in that size type
	private Vector2Int id = Vector2Int.zero;
	//the amount of debris created when destroyed
	public Vector2Int debrisAmount = new Vector2Int(3, 10);
	[SerializeField]
	private float drillDebrisChance = 0.05f, drillDustChance = 0.2f;
	#endregion

	#region Audio
	public AudioClip[] shatterSounds;
	public Vector2 shatterPitchRange;
	#endregion

	public override void Awake()
	{
		base.Awake();

		//choose size of asteroid then choose a random sprite
		id.x = Random.value <= largeChance ? 0 : 1;
		id.y = Random.Range(0, loadRes.asteroidSprites[id.x].collection.Length);
		SprRend.sprite = loadRes.asteroidSprites[id.x].collection[id.y].sprites[0];

		//if a large asteroid is chosen then adjust collider to fit the shape
		if (id.x == 0)
		{
			//set unique collider to match large shape
			ColliderInfo colInfo = largeInfo[id.y];
			switch (colInfo.type)
			{
				//circle collider
				default:
				case 0:
					((CircleCollider2D)Col[0]).radius = colInfo.size.x;
					break;
				//capsule collider
				case 1:
					GameObject obj = Col[0].gameObject;
					Destroy(Col[0]);
					CapsuleCollider2D newCol = obj.AddComponent<CapsuleCollider2D>();
					newCol.size = colInfo.size;
					newCol.offset = colInfo.offset;
					obj.transform.localEulerAngles = Vector3.forward * colInfo.rotation;
					Col[0] = newCol;
					break;
			}
			Rb.mass *= 4f;
			MaxHealth *= 4f;
		}

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
			for (int i = 0; i < Random.Range(debrisAmount.x, debrisAmount.y); i++)
			{
				CreateDebris(transform.position);
				CreateDust(transform.position);
			}

			//sound effect
			AudioManager.PlaySFX(shatterSounds[Random.Range(0, shatterSounds.Length)], transform.position,
				pitch: Random.Range(shatterPitchRange.x, shatterPitchRange.y), volume: 0.25f, parent: Shuttle.singleton.transform);

			//drop resources
			for (int i = 0; i < Random.Range(1, 4); i++)
			{
				Transform drop = Instantiate(resource).transform;
				drop.position = transform.position;
			}
		}

		base.DestroySelf();
	}

	private void CreateDebris(Vector2 pos)
	{
		int randomChoose = Random.Range(0, loadRes.debris.Length);
		if (randomChoose < loadRes.debris.Length)
		{
			ParticleGenerator.GenerateParticle(
				loadRes.debris[randomChoose], pos, shrink: false, speed: Random.value * 3f, slowDown: true,
				lifeTime: 1.5f, rotationDeg: Random.value * 360f, rotationSpeed: Random.value * 3f);
		}
	}

	private void CreateDust(Vector2 pos)
	{
		int randomChoose = Random.Range(0, loadRes.dust.Length);
		if (randomChoose < loadRes.dust.Length)
		{
			ParticleGenerator.GenerateParticle(
				loadRes.dust[randomChoose], pos, shrink: false, speed: Random.value * 0.5f, slowDown: true,
				lifeTime: Random.value * 3f + 2f, rotationDeg: Random.value * 360f, rotationSpeed: Random.value * 0.5f, size: Mathf.Pow(Random.value, 2f), alpha: Random.value * 0.1f,
				fadeIn: Random.value + 0.5f);
		}
	}

	private void UpdateSprite()
	{
		if (Health <= 0f) return;

		int delta = (int)((1f - (Health / MaxHealth)) * 3.06f);
		SprRend.sprite = GetCurrentSpriteSettings()[delta];
	}

	private NestedSpriteArray[] GetAllSprites()
	{
		return loadRes.asteroidSprites;
	}

	private SpriteArray[] GetSpriteCategory()
	{
		return GetAllSprites()[id.x].collection;
	}

	private Sprite[] GetCurrentSpriteSettings()
	{
		return GetSpriteCategory()[id.y].sprites;
	}

	// If health is below zero, this will destroy itself
	private bool CheckHealth()
	{
		UpdateSprite();
		if (Health > 0f) return false;
		DestroySelf(true);
		return true;
	}

	public bool TakeDamage(float damage, Vector2 damagePos)
	{
		//take damage
		Health -= damage;

		//particle effects
		if (Random.value < drillDebrisChance)
		{
			CreateDebris(damagePos);
		}
		if (Random.value < drillDustChance)
		{
			CreateDust(damagePos);
		}

		return CheckHealth();
	}

	//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
	public bool TakeDrillDamage(float damage, Vector2 drillPos)
	{
		bool takeDamage = TakeDamage(damage, drillPos);
		//calculate shake intensity. Gets more intense the less health it has
		ShakeFX.SetIntensity(damage / MaxHealth * (3f - (Health / MaxHealth * 2f)));

		return takeDamage;
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

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this);
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