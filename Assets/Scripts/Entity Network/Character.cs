using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
	[Header("Character Fields")]

	public Inventory storage;
	public delegate void ItemUsedEventHandler(Item.Type type);
	public event ItemUsedEventHandler OnItemUsed;
	[SerializeField] private float itemUseCooldownDuration = 1f;
	private string itemUseCooldownTimerID;

	protected override void Awake()
	{
		base.Awake();
		itemUseCooldownTimerID = gameObject.GetInstanceID() + "Item Use Cooldown Timer";
		TimerTracker.AddTimer(itemUseCooldownTimerID, 0f, null, null);
		SetShieldAmount(maxShield);
	}

	protected virtual void Update()
	{
		if (CanUseItem)
		{
			CheckItemUsageInput();
		}
	}

	protected override void OnTriggerEnter2D(Collider2D collider)
	{
		base.OnTriggerEnter2D(collider);

		Vector2 contactPoint = (collider.bounds.center - transform.position) / 2f + transform.position;

		if (collider.gameObject.layer == layerProjectile && HasShield)
		{
			IProjectile projectile = collider.GetComponent<IProjectile>();
			if (projectile.GetShooter() != this)
			{
				projectile.Hit(this, contactPoint);
			}
		}
	}

	public override bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		if (HasShield)
		{
			return TakeShieldDamage(damage, damagePos, destroyer, dropModifier, flash);
		}
		else
		{
			return base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
		}
	}

	public override ICombat GetICombat() => null;

	public virtual void ReceiveItemReward(ItemStack stack) => CollectItem(stack);

	public virtual void AcceptQuest(Quest quest)
	{
		quest.Activate();
		QuestPopupUI.ShowQuest(quest);
	}

	protected virtual void CheckItemUsageInput() { }

	protected virtual bool CheckItemUsage(int itemIndex)
	{
		List<ItemStack> stacks = storage.stacks;
		if (stacks[itemIndex].GetAmount() <= 0) return false;
		Item.Type type = stacks[itemIndex].GetItemType();
		if (!UseItem(type)) return false;
		stacks[itemIndex].RemoveAmount(1);
		return true;
	}

	protected bool UseItem(Item.Type type)
	{
		bool used = false;
		switch (type)
		{
			default: break;
			case Item.Type.RepairKit:
				used = true;
				break;
		}
		if (used)
		{
			TimerTracker.SetTimer(itemUseCooldownTimerID, itemUseCooldownDuration);
			OnItemUsed?.Invoke(type);
		}
		return used;
	}

	public virtual int CollectItem(ItemStack stack)
	{
		Item.Type itemType = stack.GetItemType();
		int amount = stack.GetAmount();
		Inventory inv = GetAppropriateInventory(itemType);
		return amount - inv.AddItem(itemType, amount);
	}

	protected virtual Inventory GetAppropriateInventory(Item.Type itemType) => storage;

	private bool CanUseItem => !ItemUsageOnCooldown;

	private bool ItemUsageOnCooldown => TimerTracker.GetTimer(itemUseCooldownTimerID) > 0f;

	public virtual bool TakeItem(Item.Type type, int amount) => false;

	protected override void DropLoot(Entity destroyer, float dropModifier)
	{
		base.DropLoot(destroyer, dropModifier);

		for (int i = 0; i < storage.stacks.Count; i++)
		{
			ItemStack stack = storage.stacks[i];
			for (int j = 0; j < stack.GetAmount(); j++)
			{
				particleGenerator.DropResource(destroyer,
					transform.position, stack.GetItemType());
			}
		}
	}

	#region Shield
	[SerializeField] protected float maxShield = 500;
	protected float currentShield = 500;
	protected bool HasShield { get { return currentShield > 0f; } }
	public float ShieldRatio { get { return maxShield > 0f ? currentShield / maxShield : 0f; } }
	[SerializeField] private EnergyShieldMaterialManager shieldVisualController;
	[SerializeField] private Collider2D shieldCol;
	public delegate void ShieldEventHandler(float oldVal, float newVal);
	public event ShieldEventHandler OnShieldUpdated;

	protected void SetShieldAmount(float value)
	{
		float oldVal = ShieldRatio;
		currentShield = Mathf.Clamp(value, 0f, maxShield);
		float newVal = ShieldRatio;
		if (oldVal != newVal)
		{
			OnShieldUpdated?.Invoke(oldVal, newVal);
		}
	}

	public virtual bool TakeShieldDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		Vector2 damageDirection = damagePos - (Vector2)transform.position;
		shieldVisualController?.TakeHit(damageDirection);

		float difference = currentShield - damage;
		SetShieldAmount(difference);
		if (difference < 0f)
		{
			return base.TakeDamage(Mathf.Abs(difference), damagePos, destroyer, dropModifier, flash);
		}
		else
		{
			return false;
		}
	}
	#endregion Shield

	#region Drill-related
	protected bool canDrill, canDrillLaunch;
	[SerializeField] protected DrillBit drill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	public DrillBit GetDrill() => canDrill ? drill : null;

	public void AttachDrill(DrillBit db) => drill = db;

	public virtual bool CanDrillLaunch() => canDrillLaunch;
	public virtual bool CanDrill() => canDrill;

	public virtual float GetLaunchDamage() => 0f;

	public virtual Vector2 LaunchDirection(Transform launchableObject) => Vector2.zero;

	public virtual bool ShouldLaunch() => false;

	//This should be overridden. Called by a drill to alert the entity that the drilling has completed
	public virtual void DrillComplete() { }

	//some entities might want to avoid drilling other entities by accident, override to verify target
	public virtual bool VerifyDrillTarget(Entity target) => true;

	//This should be overridden. Called by a drill to determine how much damage it should deal to its target.
	public virtual float DrillDamageQuery(bool firstHit) => 1f;

	public virtual float MaxDrillDamage() => 1f;

	public virtual void StoppedDrilling(bool successful) { }
	#endregion Drill-related

	#region Launch-related
	[SerializeField] private LaunchTrailController launchTrailEffect;
	[SerializeField] private GameObject drillLaunchImpactEffect;

	public virtual LaunchTrailController GetLaunchTrailAnimation() => launchTrailEffect;

	public virtual GameObject GetLaunchImpactAnimation() => drillLaunchImpactEffect;
	#endregion Launch-related
}