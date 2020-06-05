using AttackData;
using EquipmentSystem;
using InputHandlerSystem;
using InventorySystem;
using InventorySystem.UI;
using QuestSystem;
using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using TriggerSystem;
using UnityEngine;
using ValueComponents;

public class Character : Entity, IInteractor, ICrafter, IAttacker, IDeliverer
{
	[Header("Character Fields")]

	[SerializeField] private Storage defaultInventory;
	[SerializeField] private List<Storage> inventories;
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

	protected IWaypoint defaultWaypoint, currentWaypoint;
	private NarrativeManager _narrativeManager;
	protected NarrativeManager NarrativeManager => _narrativeManager != null
		? _narrativeManager
		: (_narrativeManager = FindObjectOfType<NarrativeManager>());

	public Action<Entity> OnEntityDestroyed;
	public event Action<ItemObject, int> OnItemsCollected, OnItemUsed;
	public event Action<List<ItemStack>> OnItemsCrafted;

	protected override void Initialise()
	{
		base.Initialise();

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

		if (defaultWaypoint == null)
		{
			CreateDefaultWaypoint();
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

	public RangedFloatComponent ShieldComponent => shieldValue;

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
		DefaultInventory.RemoveItemAtIndex(itemIndex, 1);
		return true;
	}

	protected virtual bool UseItem(ItemObject type)
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

	public virtual bool CanDrillLaunch => canDrillLaunch;
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
			OnItemsCollected?.Invoke(itemType, 1);
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
			OnItemsCollected?.Invoke(itemType, difference);
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

	public virtual bool StartedPerformingAction(GameAction action) => false;

	public virtual bool IsPerformingAction(GameAction action) => false;

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
		if (!CanCraftRecipe(recipe)) return false;

		List<ItemStack> ingredients = recipe.IngredientsCopy;
		List<ItemStack> results = recipe.ResultsCopy;

		RemoveItemsFromInventories(ingredients);
		AddItemsToInventories(results);

		OnItemsCrafted?.Invoke(results);
		return true;
	}

	private void AddItemsToInventories(List<ItemStack> stacks)
	{
		foreach (ItemStack stack in stacks)
		{
			ItemObject itemType = stack.ItemType;
			Storage inv = GetAppropriateInventory(itemType);
			inv.AddItem(stack);
		}
	}

	private bool RemoveItemsFromInventories(List<ItemStack> stacks)
	{
		if (!HasItems(stacks)) return false;

		foreach (ItemStack stack in stacks)
		{
			ItemObject itemType = stack.ItemType;
			Storage inv = GetAppropriateInventory(itemType);
			inv.RemoveItem(stack);
		}

		return true;
	}

	public Storage GetInventoryByName(string inventoryName)
		=> inventories.FirstOrDefault(t => t.InventoryName == inventoryName);

	public void AttachToInventoryUI()
	{
		InventoryTab.SetInventoryHolder(this);
	}

	public bool HasItem(ItemObject itemType) => HasItems(new ItemStack(itemType));

	public bool HasItems(ItemStack stack)
	{
		ItemObject itemType = stack.ItemType;
		Storage inv = GetAppropriateInventory(itemType);
		if (inv == null) return false;
		int totalCount = ItemStack.Count(inv.ItemStacks, itemType);
		int expectedAmount = stack.Amount;
		return totalCount >= expectedAmount;
	}

	public bool HasItems(List<ItemStack> stacks)
	{
		for (int i = 0; i < stacks.Count; i++)
		{
			ItemObject type = stacks[i].ItemType;
			Storage inv = GetAppropriateInventory(type);
			if (inv == null) return false;
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

	public IWaypoint GetTargetWaypoint => currentWaypoint != null ? currentWaypoint
		: defaultWaypoint;

	protected virtual void CreateDefaultWaypoint()
	{

	}

	public Vector3 Position => transform.position;

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

	public virtual bool ShouldAttack(GameAction action)
		=> !IsRecovering
		   && !TimeController.IsStopped
			&& CanAttack;

	public bool CanAttack { get; set; } = true;

	public float DamageMultiplier => damageMultiplier;

	public bool Deliver(IDelivery delivery, IDeliveryReceiver receiver)
	{
		if (!receiver.IsExpectingDelivery(this, delivery)) return false;

		if (delivery is ItemCollection collection)
		{
			List<ItemStack> stacks = collection.Stacks;
			if (!HasItems(stacks)) return false;

			bool deliveryReceived = receiver.ReceiveDelivery(this, delivery);
			if (!deliveryReceived) return false;

			RemoveItemsFromInventories(stacks);
			return true;
		}

		return false;
	}

	private const string MAX_SHIELD_VAR_NAME = "MaxShield",
		CURRENT_SHIELD_VAR_NAME = "CurrentShield",
		DEFAULT_WAYPOINT_ID_VAR_NAME = "Default Waypoint ID";

	public override void Save(string filename, SaveTag parentTag)
	{
		base.Save(filename, parentTag);

		//create save tag
		SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
		//save inventories
		foreach (Storage storage in inventories)
		{
			storage.Save(filename, mainTag);
		}
		//save default waypoint ID
		DataModule module = new DataModule(DEFAULT_WAYPOINT_ID_VAR_NAME, defaultWaypoint?.UniqueID ?? string.Empty);
		UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);

		if (shieldValue != null && ShieldIsEquipped)
		{
			//save shield max value
			module = new DataModule(MAX_SHIELD_VAR_NAME, shieldValue.UpperLimit);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save shield current value
			module = new DataModule(CURRENT_SHIELD_VAR_NAME, shieldValue.CurrentValue);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}

	protected override bool ApplyData(DataModule module)
	{
		if (base.ApplyData(module)) return true;

		switch (module.parameterName)
		{
			default:
				return false;
			case MAX_SHIELD_VAR_NAME:
			{
				bool foundMaxShield = float.TryParse(module.data, out float maxShield);
				if (foundMaxShield)
				{
					ShieldComponent.SetUpperLimit(maxShield, false);
				}
				else
				{
					Debug.Log("Max Shield data could not be parsed");
				}

				break;
			}
			case CURRENT_SHIELD_VAR_NAME:
			{
				bool foundCurrentShield = float.TryParse(module.data, out float currentShield);
				if (foundCurrentShield)
				{
					ShieldComponent.SetValue(currentShield);
				}
				else
				{
					Debug.Log("Current Shield data could not be parsed");
				}

				break;
			}
			case DEFAULT_WAYPOINT_ID_VAR_NAME:
			{
				if (module.data != string.Empty)
				{
					IUnique obj = UniqueIDGenerator.GetObjectByID(module.data);
					if (obj != null && obj is IWaypoint waypoint)
					{
						defaultWaypoint = waypoint;
					}
					else
					{
						UniqueIDGenerator.OnIDUpdated += WaitForWaypointToLoad;

						void WaitForWaypointToLoad(string ID)
						{
							if (ID != module.data) return;
							IUnique o = UniqueIDGenerator.GetObjectByID(ID);
							if (o != null && o is IWaypoint wp)
							{
								defaultWaypoint = wp;
								UniqueIDGenerator.OnIDUpdated -= WaitForWaypointToLoad;
							}

						}
						}
				}
				else
				{
					CreateDefaultWaypoint();
				}

				break;
			}
		}

		return true;
	}

	protected override bool CheckSubtag(string filename, SaveTag subtag)
	{
		if (base.CheckSubtag(filename, subtag)) return true;

		foreach (Storage storage in inventories)
		{
			if (storage.RecogniseTag(subtag))
			{
				UnifiedSaveLoad.IterateTagContents(
					filename,
					subtag,
					parameterCallBack: module => storage.ApplyData(module),
					subtagCallBack: st => storage.CheckSubtag(filename, st));
				storage.TrimPadStacks();
				return true;
			}
		}

		return false;
	}
}