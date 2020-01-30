using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using InventorySystem.UI;
using TriggerSystem;
using ValueComponents;
using DialogueSystem;
using AttackData;
using EquipmentSystem;

public class Character : Entity, IInteractor, ICrafter, IChatter, IAttacker
{
	[Header("Character Fields")]

	[SerializeField] private Storage defaultInventory;
	[SerializeField] private List<Storage> inventories;
	public event Action<ItemObject, int> OnItemCollected;
	public event Action<List<ItemStack>> OnItemsCrafted;
	public event Action<ItemObject, int> OnItemUsed;
	public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
	public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;
	public event Action<IActor> OnDisabled;

	[SerializeField] private float itemUseCooldownDuration = 1f;
	private string itemUseCooldownTimerID;
	[SerializeField] private CraftingRecipeStorage recipeStorage;

	[SerializeField] private float damageMultiplier = 1f;
	protected bool canDrill, canDrillLaunch;
	[SerializeField] protected DrillBit drill;
	public bool IsDrilling => drill == null ? false : drill.IsDrilling;
	
	[SerializeField] private GameObject shieldSlot;
	private ShieldComponent shieldComponent;
	[SerializeField] protected RangedFloatComponent shieldValue;

	[SerializeField] private FloatComponent recoveryTimerComponent;

	[SerializeField] private LaunchTrailController launchTrailEffect;
	[SerializeField] private GameObject drillLaunchImpactEffect;

	[SerializeField] private Waypoint defaultWaypoint, currentWaypoint;

	public Action<Entity> OnEntityDestroyed;

	protected override void Awake()
	{
		base.Awake();
		itemUseCooldownTimerID = gameObject.GetInstanceID() + "Item Use Cooldown Timer";
		TimerTracker.AddTimer(itemUseCooldownTimerID, 0f, null, null);
		if (ShieldIsEquipped)
		{
			shieldValue?.SetToUpperLimit();
		}
		else
		{
			shieldValue?.SetToLowerLimit();
		}
		if (!inventories.Contains(DefaultInventory))
		{
			inventories.Add(DefaultInventory);
		}
		
		SteamPunkConsole.GetCommandsFromType(GetType());
	}

	protected virtual void Update()
	{
		if (CanUseItem)
		{
			CheckItemUsageInput();
		}

		recoveryTimerComponent.SubtractValue(Time.deltaTime);
	}

	private void OnDisable() => OnDisabled?.Invoke(this);

	protected override void OnTriggerEnter2D(Collider2D collider)
	{
		base.OnTriggerEnter2D(collider);

		Vector2 contactPoint = (collider.bounds.center - transform.position) / 2f + transform.position;

		if (collider.gameObject.layer == LayerProjectile && HasShield)
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

	public void NotifyOfDestroyedEntity(Entity e) => OnEntityDestroyed?.Invoke(e);

	public override ICombat GetICombat() => null;

	protected virtual void CheckItemUsageInput() { }

	protected virtual bool CheckItemUsage(int itemIndex)
	{
		List<ItemStack> stacks = DefaultInventory.ItemStacks;
		if (stacks[itemIndex].Amount<= 0) return false;
		ItemObject type = stacks[itemIndex].ItemType;
		if (!UseItem(type)) return false;
		stacks[itemIndex].RemoveAmount(1);
		return true;
	}

	protected bool UseItem(ItemObject type)
	{
		bool used = false;
		int amountUsed = 0;

		if (type.ItemName == "Repair Kit")
		{
			used = true;
			amountUsed = 1;
			IncreaseCurrentHealth(200f);
		}

		if (used)
		{
			TimerTracker.SetTimer(itemUseCooldownTimerID, itemUseCooldownDuration);
			OnItemUsed?.Invoke(type, amountUsed);
		}
		return used;
	}

	public ref Action<ItemObject, int> GetOnItemUsedEvent => ref OnItemUsed;

	public virtual Storage GetAppropriateInventory(ItemObject itemType) => defaultInventory;

	private bool CanUseItem => !ItemUsageOnCooldown;

	private bool ItemUsageOnCooldown => TimerTracker.GetTimer(itemUseCooldownTimerID) > 0f;

	public virtual bool TakeItem(ItemObject type, int amount) => false;

	protected override void DropLoot(IInventoryHolder target, float dropModifier)
	{
		base.DropLoot(target, dropModifier);
		DropInventory(target);
	}

	protected virtual void DropInventory(IInventoryHolder target)
	{
		for (int i = 0; i < DefaultInventory.Size; i++)
		{
			ItemStack stack = DefaultInventory.ItemStacks[i];
			for (int j = 0; j < stack.Amount; j++)
			{
				DropItem(stack.ItemType, target);
			}
		}
	}

	private bool HasShield => ShieldIsEquipped
		&& shieldValue != null
		&& shieldValue.CurrentRatio > 0f;

	private bool ShieldIsEquipped
	{
		get
		{
			if (shieldSlot == null) return false;
			if (shieldComponent == null)
			{
				shieldComponent = shieldSlot.GetComponentInChildren<ShieldComponent>();
			}

			return shieldComponent != null;
		}
	}

	public virtual bool TakeShieldDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		float expectedResult = shieldValue.CurrentValue - damage;
		shieldValue.SubtractValue(damage);
		Vector3 attackDirection = (damagePos - (Vector2)transform.position).normalized;
		shieldComponent.TakeHit(attackDirection);
		if (expectedResult < 0f)
		{
			return base.TakeDamage(Mathf.Abs(expectedResult), damagePos, destroyer, dropModifier, flash);
		}
		else
		{
			return false;
		}
	}

	[SerializeField] private bool canTriggerPrompts;
	public bool CanTriggerPrompts => canTriggerPrompts;

	public CraftingRecipeStorage GetRecipeStorage => recipeStorage;

	public Storage DefaultInventory => defaultInventory;

	public List<Storage> GetAllInventories => inventories;

	public List<string> GetInventoryNames
	{
		get
		{
			List<string> names = new List<string>();
			inventories.ForEach(t => names.Add(t.InventoryName));
			return names;
		}
	}

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

	public virtual LaunchTrailController GetLaunchTrailAnimation() => launchTrailEffect;

	public virtual GameObject GetLaunchImpactAnimation() => drillLaunchImpactEffect;

	public virtual int GiveItem(ItemObject itemType)
	{
		Storage inv = GetAppropriateInventory(itemType);
		int remainingItems = inv.AddItem(itemType);
		if (remainingItems == 0)
		{
			OnItemCollected?.Invoke(itemType, 1);
		}
		else
		{
			Debug.Log($"{remainingItems} {itemType} not added");
		}
		return remainingItems;
	}

	public virtual int GiveItem(ItemStack stack)
	{
		ItemObject itemType = stack.ItemType;
		int amount = stack.Amount;
		Storage inv = GetAppropriateInventory(itemType);
		int remainingItems = inv.AddItem(itemType, amount);
		int difference = amount - remainingItems;
		if (difference != 0)
		{
			OnItemCollected?.Invoke(itemType, difference);
		}
		return remainingItems;
	}

	[SteamPunkConsoleCommand(command = "GiveItem", info = "Syntax: giveitem <itemName> <?amount>\n" +
		"Gives an item to the Shuttle")]
	public void GiveItemCommand(string itemName, int amount)
	{
		ItemObject itemType = Item.GetItemByName(itemName);
		bool successful = itemType != null;
		if (!successful)
		{
			Debug.Log($"No Item with name {itemName} found.");
			return;
		}
		Debug.Log($"Giving {amount} {itemType} to {GetType().Name}", gameObject);
		GiveItem(new ItemStack(itemType, amount));
	}

	public virtual void EnteredTrigger(ITrigger vTrigger)
	{

	}

	public virtual void ExitedTrigger(ITrigger vTrigger)
	{

	}

	public virtual bool StartedPerformingAction(string action) => false;

	public virtual bool IsPerformingAction(string action) => false;

	public virtual object ObjectOrderRequest(object order)
	{
		if (order is ItemObject itemType)
		{
			Storage inv = GetAppropriateInventory(itemType);
			bool removed = inv.RemoveItem(itemType);
			return removed;
		}
		if (order is ItemStack stack)
		{
			Storage inv = GetAppropriateInventory(stack.ItemType);
			bool removed = inv.RemoveItem(stack);
			return removed;
		}

		return null;
	}

	public virtual void Interact(object interactableObject)
	{
		if (interactableObject is ItemObject itemType)
		{
			defaultInventory.AddItem(itemType);
			return;
		}

		if (interactableObject is ItemStack stack)
		{
			defaultInventory.AddItem(stack);
			return;
		}
	}

	public bool Craft(CraftingRecipe recipe)
	{
		List<ItemStack> ingredients = recipe.IngredientsCopy;
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemStack stack = ingredients[i];
			ItemObject type = stack.ItemType;
			Storage inv = GetAppropriateInventory(type);
			if (ItemStack.Count(inv.ItemStacks, type) < stack.Amount) return false;
		}

		defaultInventory.RemoveItems(ingredients);
		List<ItemStack> results = recipe.ResultsCopy;
		if (!defaultInventory.CanFit(results))
		{
			defaultInventory.AddItems(ingredients);
			return false;
		}

		OnItemsCrafted?.Invoke(results);
		defaultInventory.AddItems(results);
		return true;
	}

	public Storage GetInventoryByName(string inventoryName)
		=> inventories.FirstOrDefault(t => t.InventoryName == inventoryName);

	public void AttachToInventoryUI()
	{
		InventoryTab.SetInventoryHolder(this);
	}

	public bool HasItems(List<ItemStack> stacks)
	{
		for (int i = 0; i < stacks.Count; i++)
		{
			ItemObject type = stacks[i].ItemType;
			Storage inv = GetAppropriateInventory(type);
			int expectedAmount = ItemStack.Count(stacks, type);
			if (ItemStack.Count(inv.ItemStacks, type) < expectedAmount) return false;
		}
		return true;
	}

	public bool CanCraftRecipe(CraftingRecipe recipe)
	{
		List<ItemStack> ingredients = recipe.IngredientsCopy;
		if (!HasItems(ingredients)) return false;

		List<Storage> relatedInventories = new List<Storage>();
		List<ItemStack> results = recipe.ResultsCopy;
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemObject type = ingredients[i].ItemType;
			Storage inv = GetAppropriateInventory(type);
			if (relatedInventories.Contains(inv)) continue;
			List<ItemStack> invStacks = inv.CreateCopyOfStacks();
			ItemStack.RemoveItems(invStacks, ingredients);
			relatedInventories.Add(inv);

			for (int j = 0; j < results.Count; j++)
			{
				ItemObject resultItemType = results[i].ItemType;
				Storage inv2 = GetAppropriateInventory(resultItemType);
				if (inv != inv2) continue;
				if (!ItemStack.CanFit(invStacks, results[i])) return false;
			}
		}
		return true;
	}

	public Waypoint GetWaypoint => currentWaypoint != null ? currentWaypoint
		: defaultWaypoint;

	public Vector3 Position => transform.position;

	protected void SendActiveDialogue(ConversationWithActions dialogue, bool skip)
	{
		OnSendActiveDialogue?.Invoke(dialogue, skip);
	}

	protected void SendPassiveDialogue(ConversationWithActions dialogue, bool skip)
	{
		OnSendPassiveDialogue?.Invoke(dialogue, skip);
	}

	public virtual void ReceiveRecoil(Vector3 recoilVector)
	{
		rb.AddForce(recoilVector);
	}

	public virtual void ReceiveStoppingPower(float stoppingPower)
	{
		rb.velocity *= 1f - stoppingPower;
	}

	public void ReceiveRecoveryDuration(float recoveryDuration)
	{
		float value = Mathf.Max(recoveryDuration, recoveryTimerComponent.CurrentValue);
		recoveryTimerComponent.SetValue(value);
	}

	protected bool IsRecovering => recoveryTimerComponent.CurrentValue > 0f;

	public virtual bool ShouldAttack(string action)
		=> !IsRecovering
		   && !Pause.IsStopped
			&& CanAttack;

	public void AllowSendingDialogue(bool allow)
	{
		CanSendDialogue = allow;
	}

	public bool CanAttack { get; set; } = true;

	public float DamageMultiplier => damageMultiplier;

	public bool CanSendDialogue { get; set; } = true;
}