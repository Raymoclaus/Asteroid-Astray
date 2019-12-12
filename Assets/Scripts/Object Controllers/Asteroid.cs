using AudioUtilities;
using CustomDataTypes;
using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Entity
{
	[Header("Asteroid Fields")]
	[Tooltip("Reference to the shake effect script on the sprite.")]
	public ShakeEffect ShakeFX;
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float SpinSpeedRange;
	[Tooltip("Picks a random value between given value and negative given value to determine starting velocity")]
	public float VelocityRange;
	[SerializeField] private SpriteRenderer sprRend;
	[SerializeField] private List<Sprite> debrisSprites;
	[SerializeField] private int debrisSortingOrder;
	//the amount of debris created when destroyed
	public IntPair debrisAmount = new IntPair(3, 10);
	[SerializeField] private float drillDebrisChance = 0.05f;
	[SerializeField] private List<Sprite> dustSprites;
	[SerializeField] private int dustSortingOrder;
	[SerializeField] private float drillDustChance = 0.2f;

	public AudioClip[] shatterSounds;
	public Vector2 shatterPitchRange;
	[SerializeField]
	private AudioSO collisionSounds;

	[SerializeField] private LimitedScriptedDrops scriptedDrops;

	protected override void Awake()
	{
		base.Awake();
		RandomMovement();
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
			if (AudioMngr != null)
			{
				AudioMngr.PlaySFX(shatterSounds[Random.Range(0, shatterSounds.Length)], transform.position,
					pitch: Random.Range(shatterPitchRange.x, shatterPitchRange.y), volume: 0.25f,
					parent: null);
			}
		}
		EjectFromAllDrillers(true);
		base.DestroySelf(destroyer, dropModifier);
	}

	protected override void DropLoot(IInventoryHolder target, float dropModifier)
	{
		base.DropLoot(target, dropModifier);

		if (scriptedDrops.scriptedDropsActive)
		{
			LootGroup group = scriptedDrops.GetScriptedDrop(target);
			List<ItemStack> stacks = group.GetStacks;
			if (stacks != null)
			{
				for (int i = 0; i < stacks.Count; i++)
				{
					ItemStack stack = stacks[i];
					for (int j = 0; j < stack.Amount; j++)
					{
						DropItem(stack.ItemType, target);
					}
				}
				return;
			}
		}
	}

	private void CreateDebris(Vector2 pos)
	{
		if (!IsInViewRange || Pause.IsStopped) return;

		if (!PartGen) return;

		int randomChoose = Random.Range(0, debrisSprites.Count);
		PartGen.GenerateParticle(
			debrisSprites[randomChoose], pos, speed: Random.value * 3f, slowDown: true, lifeTime: 1.5f,
			rotationDeg: Random.value * 360f, rotationSpeed: Random.value * 3f,
			sortingLayer: sprRend.sortingLayerID, sortingOrder: debrisSortingOrder);
	}

	private void CreateDust(Vector2 pos, float alpha = 0.1f)
	{
		if (!IsInViewRange || Pause.IsStopped) return;
		
		if (PartGen == null) return;

		int randomChoose = Random.Range(0, dustSprites.Count);
		PartGen.GenerateParticle(
			dustSprites[randomChoose], pos, speed: Random.value * 0.5f, slowDown: true,
			lifeTime: Random.value * 3f + 2f, rotationDeg: Random.value * 360f,
			rotationSpeed: Random.value * 0.5f,
			size: 0.3f + Mathf.Pow(Random.value, 2f) * 0.7f, alpha: alpha,
			fadeIn: Random.value + 0.5f, sortingLayer: sprRend.sortingLayerID,
			growthOverLifetime: 2f, sortingOrder: dustSortingOrder);
	}

	private void UpdateSprite()
	{
		float hpRatio = healthComponent.CurrentRatio;

	}

	protected override void OnEnterPhysicsRange()
	{
		base.OnEnterPhysicsRange();
		Vector2 newDir = -rb.velocity;
		newDir.Normalize();
		rb.velocity = newDir * VelocityRange;
	}

	protected override void OnEnterViewRange()
	{
		base.OnEnterViewRange();
		//ActivateAllColliders(true);
	}

	protected override void OnExitViewRange()
	{
		base.OnExitViewRange();
		//ActivateAllColliders(false);
	}

	// If health is below zero, this will destroy itself
	protected override bool CheckHealth(Entity destroyer, float dropModifier)
	{
		UpdateSprite();
		if (healthComponent.CurrentRatio > 0f) return false;
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
		ShakeFX.SetIntensity(damage / healthComponent.UpperLimit * (3f - (healthComponent.CurrentRatio * 2f)));
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

		CreateDust(contactPoint, 0.1f + Random.value * 0.2f);
	}

	public override void Launch(Vector2 launchDirection, Character launcher)
	{
		base.Launch(launchDirection, launcher);
		ShakeFX.Begin(0.1f, 0f, 1f / 30f);
	}
}