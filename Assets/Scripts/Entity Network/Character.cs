using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using TriggerSystem;
using ValueComponents;
using InventorySystem.UI;

public class Character : Entity, IInteractor, ICrafter
{
	[Header("Character Fields")]

	[SerializeField] private Inventory defaultInventory;
	[SerializeField] private List<Inventory> inventories;
	public event Action<Item.Type, int> OnItemCollected;
	public event Action<List<ItemStack>> OnItemsCrafted;
	public event Action<Item.Type, int> OnItemUsed;
	[SerializeField] private float itemUseCooldownDuration = 1f;
	private string itemUseCooldownTimerID;
	[SerializeField] private CraftingRecipeStorage recipeStorage;

	protected bool canDrill, canDrillLaunch;
	[SerializeField] protected DrillBit drill;
	public bool IsDrilling => drill == null ? false : drill.IsDrilling;

	[SerializeField] private LaunchTrailController launchTrailEffect;
	[SerializeField] private GameObject drillLaunchImpactEffect;

	public Action<Entity> OnEntityDestroyed;

	protected override void Awake()
	{
		base.Awake();
		itemUseCooldownTimerID = gameObject.GetInstanceID() + "Item Use Cooldown Timer";
		TimerTracker.AddTimer(itemUseCooldownTimerID, 0f, null, null);
		shieldComponent?.SetToUpperLimit();
		if (!inventories.Contains(DefaultInventory))
		{
			inventories.Add(DefaultInventory);
		}

		SteamPunkConsole spc = FindObjectOfType<SteamPunkConsole>();
		spc?.GetCommandsFromType(GetType());
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

	public void NotifyOfDestroyedEntity(Entity e) => OnEntityDestroyed?.Invoke(e);

	public override ICombat GetICombat() => null;

	protected virtual void CheckItemUsageInput() { }

	protected virtual bool CheckItemUsage(int itemIndex)
	{
		List<ItemStack> stacks = DefaultInventory.ItemStacks;
		if (stacks[itemIndex].GetAmount() <= 0) return false;
		Item.Type type = stacks[itemIndex].GetItemType();
		if (!UseItem(type)) return false;
		stacks[itemIndex].RemoveAmount(1);
		return true;
	}

	protected bool UseItem(Item.Type type)
	{
		bool used = false;
		int amountUsed = 0;
		switch (type)
		{
			default: break;
			case Item.Type.RepairKit:
				used = true;
				amountUsed = 1;
				break;
		}
		if (used)
		{
			TimerTracker.SetTimer(itemUseCooldownTimerID, itemUseCooldownDuration);
			OnItemUsed?.Invoke(type, amountUsed);
		}
		return used;
	}

	public ref Action<Item.Type, int> GetOnItemUsedEvent => ref OnItemUsed;

	public virtual Inventory GetAppropriateInventory(Item.Type itemType) => defaultInventory;

	private bool CanUseItem => !ItemUsageOnCooldown;

	private bool ItemUsageOnCooldown => TimerTracker.GetTimer(itemUseCooldownTimerID) > 0f;

	public virtual bool TakeItem(Item.Type type, int amount) => false;

	protected override void DropLoot(IInventoryHolder target, float dropModifier)
	{
		base.DropLoot(target, dropModifier);

		for (int i = 0; i < DefaultInventory.Size; i++)
		{
			ItemStack stack = DefaultInventory.ItemStacks[i];
			for (int j = 0; j < stack.GetAmount(); j++)
			{
				PartGen.DropResource(target,
					transform.position, stack.GetItemType());
			}
		}
	}

	[SerializeField] protected RangedFloatComponent shieldComponent;
	private bool HasShield => shieldComponent != null ? shieldComponent.Ratio > 0f : false;

	public virtual bool TakeShieldDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		float expectedResult = shieldComponent.currentValue - damage;
		shieldComponent.SubtractValue(damage);
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

	public Inventory DefaultInventory => defaultInventory;

	public List<Inventory> GetAllInventories => inventories;

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

	public virtual int GiveItem(Item.Type itemType)
	{
		Inventory inv = GetAppropriateInventory(itemType);
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
		Item.Type itemType = stack.GetItemType();
		int amount = stack.GetAmount();
		Inventory inv = GetAppropriateInventory(itemType);
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
		bool successful = Enum.TryParse(itemName, true, out Item.Type itemType);
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

	public virtual bool IsPerformingAction(string action)
	{
		return false;
	}

	public virtual object ObjectOrderRequest(object order)
	{
		if (order is Item.Type itemType)
		{
			Inventory inv = GetAppropriateInventory(itemType);
			bool removed = inv.RemoveItem(itemType);
			return removed;
		}
		if (order is ItemStack stack)
		{
			Inventory inv = GetAppropriateInventory(stack.GetItemType());
			bool removed = inv.RemoveItem(stack);
			return removed;
		}

		return null;
	}

	public virtual void Interact(object interactableObject)
	{
		if (interactableObject is Item.Type itemType)
		{
			defaultInventory.AddItem(itemType);
		}
		if (interactableObject is ItemStack stack)
		{
			defaultInventory.AddItem(stack);
		}
	}

	public bool Craft(CraftingRecipe recipe)
	{
		List<ItemStack> ingredients = recipe.Ingredients;
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemStack stack = ingredients[i];
			if (defaultInventory.Count(stack.GetItemType()) < stack.GetAmount()) return false;
		}

		defaultInventory.RemoveItems(ingredients);
		List<ItemStack> results = recipe.Results;
		if (!defaultInventory.CanFit(results))
		{
			defaultInventory.AddItems(ingredients);
			return false;
		}

		defaultInventory.AddItems(results);
		OnItemsCrafted?.Invoke(results);
		return true;
	}

	public Inventory GetInventoryByName(string inventoryName)
		=> inventories.FirstOrDefault(t => t.InventoryName == inventoryName);

	public void AttachToInventoryUI()
	{
		InventoryUIController.SetInventoryHolder(this);
	}
}