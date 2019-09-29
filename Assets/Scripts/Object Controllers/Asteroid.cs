using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Entity
{
	[System.Serializable]
	private struct ColliderInfo
	{
		public int type;
		public Vector2 size;
		public Vector2 offset;
		public float rotation;
	}

	[Header("Asteroid Fields")]

	#region Fields
	[Tooltip("Reference to the sprite renderer of the asteroid.")]
	public SpriteRenderer SprRend;
	[Tooltip("Reference to the shake effect script on the sprite.")]
	public ShakeEffect ShakeFX;
	[SerializeField]
	private AsteroidSprites spritesSO;
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float SpinSpeedRange;
	[Tooltip("Picks a random value between given value and negative given value to determine starting velocity")]
	public float VelocityRange;
	//collection of info about the colliders the large asteroids should use
	[SerializeField]
	private ColliderInfo[] largeInfo;
	//chance that an asteroid will be larger than normal
	[SerializeField]
	private float largeChance = 0.01f;
	private bool isLarge;
	//keeps track of size type and ID in that size type
	private IntPair id = IntPair.zero;
	//the amount of debris created when destroyed
	public IntPair debrisAmount = new IntPair(3, 10);
	[SerializeField] private float drillDebrisChance = 0.05f, drillDustChance = 0.2f;
	private int collisionDustMultiplier = 3;
	#endregion

	#region Audio
	public AudioClip[] shatterSounds;
	public Vector2 shatterPitchRange;
	[SerializeField]
	private AudioSO collisionSounds;
	#endregion

	protected override void Awake()
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
			maxHP *= 4f;
		}

		RandomMovement();

		//start health at max value
		currentHP = maxHP;

		initialised = true;
	}

	public void OnEnable() => RandomMovement();

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

	public override void DestroySelf(Entity destroyer, float dropModifier)
	{
		bool explode = destroyer != null;
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
		}
		EjectFromAllDrillers(true);
		base.DestroySelf(destroyer, dropModifier);
	}

	protected override void DropLoot(Entity destroyer, float dropModifier)
	{
		base.DropLoot(destroyer, dropModifier);

		if (FirstQuestScriptedDrops.scriptedDropsActive)
		{
			List<ItemStack> stacks = FirstQuestScriptedDrops.GetScriptedDrop(destroyer);
			if (stacks != null)
			{
				for (int i = 0; i < stacks.Count; i++)
				{
					ItemStack stack = stacks[i];
					for (int j = 0; j < stack.GetAmount(); j++)
					{
						particleGenerator.DropResource(destroyer,
							transform.position, stack.GetItemType());
					}
				}
				return;
			}
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

		if (currentHP <= 0f)
		{
			particleGenerator.GenerateParticle(GetCurrentSpriteSettings()[GetCurrentSpriteSettings().Length - 1],
				transform.position, fadeOut: false, lifeTime: 0.05f,
				rotationDeg: transform.eulerAngles.z, sortingLayer: SprRend.sortingLayerID);
			return;
		}

		int delta = (int)((1f - (currentHP / maxHP)) * GetCurrentSpriteSettings().Length - 1);
		delta = Mathf.Clamp(delta, 0, GetCurrentSpriteSettings().Length - 1);
		SprRend.sprite = GetCurrentSpriteSettings()[delta];
	}

	private NestedSpriteArray[] GetAllSprites() => spritesSO.asteroidSprites;

	private SpriteArray[] GetSpriteCategory() => GetAllSprites()[id.x].collection;

	private Sprite[] GetCurrentSpriteSettings() => GetSpriteCategory()[id.y].sprites;

	public override bool OnExitPhysicsRange()
	{
		Vector2 newDir = -rb.velocity;
		newDir.Normalize();
		rb.velocity = newDir * VelocityRange;
		return true;
	}

	// If health is below zero, this will destroy itself
	protected override bool CheckHealth(Entity destroyer, float dropModifier)
	{
		UpdateSprite();
		if (currentHP > 0f) return false;
		return base.CheckHealth(destroyer, dropModifier);
	}

	public override bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		//particle effects
		if (Random.value < drillDebrisChance)
		{
			CreateDebris(damagePos);
		}
		if (Random.value < drillDustChance)
		{
			CreateDust(damagePos, alpha: Random.value * 0.1f);
		}

		return base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
	}

	//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
	public override bool TakeDrillDamage(float damage, Vector2 drillPos,
		Entity destroyer, float dropModifier)
	{
		//calculate shake intensity. Gets more intense the less health it has
		ShakeFX.SetIntensity(damage / maxHP * (3f - (currentHP / maxHP * 2f)));
		return base.TakeDrillDamage(damage, drillPos, destroyer, dropModifier);
	}

	public override void StartDrilling(DrillBit db)
	{
		base.StartDrilling(db);
		ShakeFX.Begin();
	}

	public override void StopDrilling(DrillBit db)
	{
		base.StopDrilling(db);
		ShakeFX.Stop();
	}

	//If queried, this object will say that it is an asteroid-type Entity
	public override EntityType GetEntityType() => EntityType.Asteroid;

	protected override void OnCollisionEnter2D(Collision2D collision)
	{
		base.OnCollisionEnter2D(collision);

		float collisionStrength = collision.relativeVelocity.magnitude;
		if (collisionStrength < 0.1f) return;

		Collider2D other = collision.collider;
		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;

		CreateDust(contactPoint, (int)collisionStrength * collisionDustMultiplier, 0.1f + Random.value * 0.2f);
	}

	public override void Launch(Vector2 launchDirection, Character launcher)
	{
		base.Launch(launchDirection, launcher);
		ShakeFX.Begin(0.1f, 0f, 1f / 30f);
	}
}