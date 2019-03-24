using System.Collections.Generic;
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
	[Tooltip("Reference to the shake effect script on the sprite.")]
	public ShakeEffect ShakeFX;
	private CameraCtrl cameraCtrl;
	[SerializeField]
	private AsteroidSprites spritesSO;
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float SpinSpeedRange;
	[Tooltip("Picks a random value between given value and negative given value to determine starting velocity")]
	public float VelocityRange;
	[Tooltip("Upper limit for health stat.")]
	public float MaxHealth = 150f;
	//current health value between 0 and MaxHealth
	private float Health;
	//collection of info about the colliders the large asteroids should use
	[SerializeField]
	private ColliderInfo[] largeInfo;
	//chance that an asteroid will be larger than normal
	[SerializeField]
	private float largeChance = 0.01f;
	private bool isLarge;
	//keeps track of size type and ID in that size type
	private Vector2Int id = Vector2Int.zero;
	private List<DrillBit> drillers = new List<DrillBit>();
	//the amount of debris created when destroyed
	public Vector2Int debrisAmount = new Vector2Int(3, 10);
	[SerializeField] private float drillDebrisChance = 0.05f, drillDustChance = 0.2f;
	private int collisionDustMultiplier = 3;
	private bool launched = false;
	[SerializeField] private float launchDuration = 1.5f;
	private Character launcher;
	private LaunchTrailController launchTrail;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	[SerializeField] private List<Loot> loot;
	#endregion

	#region Audio
	public AudioClip[] shatterSounds;
	public Vector2 shatterPitchRange;
	[SerializeField]
	private AudioSO collisionSounds;
	#endregion

	public override void Awake()
	{
		base.Awake();
		
		id.x = Random.value <= largeChance ? 0 : 1;
		id.y = Random.Range(0, spritesSO.asteroidSprites[id.x].collection.Length);
		SprRend.sprite = spritesSO.asteroidSprites[id.x].collection[id.y].sprites[0];

		//if a large asteroid is chosen then adjust collider to fit the shape
		if (id.x == 0)
		{
			//set unique collider to match large shape
			ColliderInfo colInfo = largeInfo[id.y];
			isLarge = true;
			switch (colInfo.type)
			{
				//circle collider
				default:
				case 0:
					((CircleCollider2D)col[0]).radius = colInfo.size.x;
					break;
				//capsule collider
				case 1:
					GameObject obj = col[0].gameObject;
					Destroy(col[0]);
					CapsuleCollider2D newCol = obj.AddComponent<CapsuleCollider2D>();
					newCol.size = colInfo.size;
					newCol.offset = colInfo.offset;
					obj.transform.localEulerAngles = Vector3.forward * colInfo.rotation;
					col[0] = newCol;
					break;
			}
			rb.mass *= 4f;
			MaxHealth *= 4f;
		}

		RandomMovement();

		//start health at max value
		Health = MaxHealth;

		initialised = true;
	}

	public void OnEnable()
	{
		RandomMovement();
	}

	private void RandomMovement()
	{
		//picks a random speed to spin at within a given range with chance favoring lower values
		rb.AddTorque((Mathf.Pow(Random.Range(0f, 2f), 2f) * SpinSpeedRange - SpinSpeedRange));
		//picks a random direction and velocity within a given range with chance favoring lower values
		rb.velocity = new Vector2(
			Mathf.Sin(Random.value * 2f * Mathf.PI),
			Mathf.Cos(Random.value * 2f * Mathf.PI))
			* (Mathf.Pow(Random.Range(0f, 2f), 2f) * VelocityRange - VelocityRange);
	}

	private void DestroySelf(bool explode, Entity destroyer, int dropModifier = 0)
	{
		if (explode)
		{
			//particle effect
			for (int i = 0; i < Random.Range(debrisAmount.x, debrisAmount.y); i++)
			{
				CreateDebris(transform.position);
				CreateDust(transform.position, alpha: 0.3f);
			}

			//sound effect
			audioManager = audioManager ?? FindObjectOfType<AudioManager>();
			if (audioManager)
			{
				audioManager.PlaySFX(shatterSounds[Random.Range(0, shatterSounds.Length)], transform.position,
					pitch: Random.Range(shatterPitchRange.x, shatterPitchRange.y), volume: 0.25f,
					parent: null);
			}

			//drop resources
			DropLoot(destroyer, transform.position, dropModifier);
		}
		EjectFromAllDrillers();
		base.DestroySelf(destroyer);
	}

	private void DropLoot(Entity destroyer, Vector2 pos, int dropModifier = 0)
	{
		particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();

		int minDrop = isLarge ? 4 : 1;
		for (int i = 0; i < Random.Range(minDrop, minDrop + dropModifier + 1); i++)
		{
			particleGenerator.DropResource(destroyer, pos);
		}

		if (FirstQuestScriptedDrops.scriptedDropsActive)
		{
			List<ItemStack> stacks = FirstQuestScriptedDrops.GetScriptedDrop(destroyer);
			if (stacks != null)
			{
				for (int i = 0; i < stacks.Count; i++)
				{
					ItemStack stack = stacks[i];
					particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
				}
				return;
			}
		}

		for (int i = 0; i < loot.Count; i++)
		{
			ItemStack stack = loot[i].GetStack();
			particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
		}
	}

	private void CreateDebris(Vector2 pos)
	{
		if (!isActive || Pause.IsStopped) return;

		particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();
		if (!particleGenerator) return;

		int randomChoose = Random.Range(0, spritesSO.debris.Length);
		if (randomChoose < spritesSO.debris.Length)
		{
			particleGenerator.GenerateParticle(
				spritesSO.debris[randomChoose], pos, speed: Random.value * 3f, slowDown: true, lifeTime: 1.5f,
				rotationDeg: Random.value * 360f, rotationSpeed: Random.value * 3f,
				sortingLayer: SprRend.sortingLayerID, sortingOrder: -1);
		}
	}

	private void CreateDust(Vector2 pos, int amount = 1, float alpha = 0.1f)
	{
		if (!isActive || Pause.IsStopped) return;

		//particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();
		if (!particleGenerator) return;

		for (int i = 0; i < amount; i++)
		{
			int randomChoose = Random.Range(0, spritesSO.dust.Length);
			if (randomChoose < spritesSO.dust.Length)
			{
				particleGenerator.GenerateParticle(
					spritesSO.dust[randomChoose], pos, speed: Random.value * 0.5f, slowDown: true,
					lifeTime: Random.value * 3f + 2f, rotationDeg: Random.value * 360f,
					rotationSpeed: Random.value * 0.5f, size: 0.3f + Mathf.Pow(Random.value, 2f) * 0.7f, alpha: alpha,
					fadeIn: Random.value + 0.5f, sortingLayer: SprRend.sortingLayerID, growthOverLifetime: 2f);
			}
		}
	}

	private void UpdateSprite()
	{
		particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();

		if (Health <= 0f)
		{
			particleGenerator.GenerateParticle(GetCurrentSpriteSettings()[GetCurrentSpriteSettings().Length - 1],
				transform.position, fadeOut: false, lifeTime: 0.05f,
				rotationDeg: transform.eulerAngles.z, sortingLayer: SprRend.sortingLayerID);
			return;
		}

		int delta = (int)((1f - (Health / MaxHealth)) * GetCurrentSpriteSettings().Length - 1);
		delta = Mathf.Clamp(delta, 0, GetCurrentSpriteSettings().Length - 1);
		SprRend.sprite = GetCurrentSpriteSettings()[delta];
	}

	private NestedSpriteArray[] GetAllSprites()
	{
		return spritesSO.asteroidSprites;
	}

	private SpriteArray[] GetSpriteCategory()
	{
		return GetAllSprites()[id.x].collection;
	}

	private Sprite[] GetCurrentSpriteSettings()
	{
		return GetSpriteCategory()[id.y].sprites;
	}

	public override bool OnExitPhysicsRange()
	{
		Vector2 newDir = -rb.velocity;
		newDir.Normalize();
		rb.velocity = newDir * VelocityRange;
		return true;
	}

	// If health is below zero, this will destroy itself
	private bool CheckHealth(Entity destroyer, int dropModifier = 0)
	{
		UpdateSprite();
		if (Health > 0f) return false;
		DestroySelf(true, destroyer, dropModifier);
		return true;
	}

	public Vector2 GetPosition()
	{
		return transform.position;
	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0, bool flash = true)
	{
		if (Health <= 0f) return false;
		//take damage
		Health -= damage;

		//particle effects
		if (Random.value < drillDebrisChance)
		{
			CreateDebris(damagePos);
		}
		if (Random.value < drillDustChance)
		{
			CreateDust(damagePos, alpha: Random.value * 0.1f);
		}

		return CheckHealth(destroyer, dropModifier);
	}

	//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
	public bool TakeDrillDamage(float damage, Vector2 drillPos, Entity destroyer, int dropModifier = 0)
	{
		if (TakeDamage(damage, drillPos, destroyer, dropModifier))
		{
			//calculate shake intensity. Gets more intense the less health it has
			ShakeFX.SetIntensity(damage / MaxHealth * (3f - (Health / MaxHealth * 2f)));
			return true;
		}
		return false;
	}

	public void StartDrilling(DrillBit db)
	{
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
		ShakeFX.Begin();
		AddDriller(db);
	}
	
	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && otherDrill.drillTarget == null && otherDrill.Verify(this))
			{
				StartDrilling(otherDrill);
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
				StopDrilling(otherDrill);
				otherDrill.StopDrilling();
			}
		}
	}

	public void StopDrilling(DrillBit db)
	{
		rb.constraints = RigidbodyConstraints2D.None;
		ShakeFX.Stop();
		RemoveDriller(db);
	}

	//If queried, this object will say that it is an asteroid-type Entity
	public override EntityType GetEntityType()
	{
		return EntityType.Asteroid;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		float collisionStrength = collision.relativeVelocity.magnitude;
		if (collisionStrength < 0.1f) return;

		Collider2D other = collision.collider;
		int otherLayer = other.gameObject.layer;
		//collision.GetContacts(contacts);
		//Vector2 contactPoint = contacts[0].point;
		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;
		float angle = -Vector2.SignedAngle(Vector2.up, contactPoint - (Vector2)transform.position);

		//dust particle effect
		CreateDust(contactPoint, (int)collisionStrength * collisionDustMultiplier, 0.1f + Random.value * 0.2f);

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
		}

		if (otherLayer == layerSolid)
		{
			if (launched)
			{
				if (launcher.GetLaunchImpactAnimation() != null)
				{
					Transform impact = Instantiate(launcher.GetLaunchImpactAnimation()).transform;
					impact.parent = ParticleGenerator.holder;
					impact.position = contactPoint;
					impact.eulerAngles = Vector3.forward * angle;
				}
				if (launchTrail != null)
				{
					launchTrail.CutLaunchTrail();
				}
				IDamageable otherDamageable = other.attachedRigidbody.gameObject.GetComponent<IDamageable>();
				float damage = launcher.GetLaunchDamage();
				if (Health / MaxHealth < 0.7f)
				{
					damage *= 2f;
				}
				otherDamageable?.TakeDamage(damage, contactPoint, launcher);
				TakeDamage(damage, contactPoint, launcher);
				launched = false;
				if (cameraCtrl && cameraCtrl.GetPanTarget() == transform)
				{
					cameraCtrl.Pan(null);
				}
			}
		}
	}

	public void Launch(Vector2 launchDirection, Character launcher)
	{
		this.launcher = launcher;
		rb.velocity = launchDirection;
		launched = true;
		ShakeFX.Begin(0.1f, 0f, 1f / 30f);
		if (cameraCtrl) cameraCtrl.Pan(transform);
		if (launcher.GetLaunchTrailAnimation() != null)
		{
			launchTrail = Instantiate(launcher.GetLaunchTrailAnimation());
			launchTrail.SetFollowTarget(transform, launchDirection, isLarge ? 2f : 1f);

		}
		pause = pause ?? FindObjectOfType<Pause>();
		if (pause)
		{
			pause.DelayedAction(() =>
			{
				if (launchTrail != null)
				{
					launchTrail.EndLaunchTrail();
				}

				this.launcher = null;
				launched = false;
				if (cameraCtrl && cameraCtrl.GetPanTarget() == transform)
				{
					cameraCtrl.Pan(null);
				}
			}, launchDuration, true);
		}
	}

	public bool IsDrillable()
	{
		return launched && Health > 0f;
	}

	public bool CanBeLaunched()
	{
		return Health > 0f;
	}

	public List<DrillBit> GetDrillers()
	{
		return drillers;
	}

	public void AddDriller(DrillBit db)
	{
		GetDrillers().Add(db);
	}

	public bool RemoveDriller(DrillBit db)
	{
		List<DrillBit> drills = GetDrillers();
		for (int i = 0; i < drills.Count; i++)
		{
			if (drills[i] == db)
			{
				drills.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	private void EjectFromAllDrillers()
	{
		List<DrillBit> drills = GetDrillers();
		for (int i = drills.Count - 1; i >= 0; i--)
		{
			drills[i].StopDrilling();
		}
	}
}