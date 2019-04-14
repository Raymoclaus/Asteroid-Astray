using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(HiveInventory))]
public class BotHive : Character, ICombat
{
	[Header("Bot Hive Fields")]

	#region Fields
	//references
	[SerializeField] private GatherBot botPrefab;
	[SerializeField] private HiveInventory inventory;
	[SerializeField] private Transform dockHolder;
	private Transform[] docks;
	[SerializeField] private Animator[] dockAnims;
	[SerializeField] private AudioSO collisionSounds;
	[SerializeField] private GameObject burningEffects;
	[SerializeField] private GameObject explosionEffects;
	[SerializeField] private SpriteRenderer sprRend;
	[SerializeField] private Sprite burningSprite;

	//fields
	[SerializeField] private float botBaseHP = 500f;
	[HideInInspector] public List<GatherBot> childBots = new List<GatherBot>();
	private bool[] occupiedDocks;
	[SerializeField] private int minInitialBotCount = 2, maxBotCount = 3, minLeftoverResources = 1,
		botCreationCost = 2, botUpgradeCost = 4, maxInitialUpgrades = 0;
	private int resourceCount, toBeSpent;
	private bool dormant;
	private int needToCreate, needToUpgrade;
	private List<ChunkCoords> emptyCoords = new List<ChunkCoords>();
	private List<GTime> emptyCoordTimes = new List<GTime>();
	private float emptyCoordWaitTime = 300f;
	public WaitForSeconds maintenanceTime = new WaitForSeconds(3f);
	private List<ICombat> enemies = new List<ICombat>();
	[SerializeField] private List<Loot> lootDrops;
	[SerializeField] private float burnTime = 3f;
	private float burnTimer = 0f;
	private bool burning = false;
	private Entity destroyerEntity;
	private int dropModifierOnDeath;

	//cache
	private List<ChunkCoords> botOccupiedCoords = new List<ChunkCoords>();
	private List<ChunkCoords> searchCoords = new List<ChunkCoords>();
	#endregion

	private void Start()
	{
		occupiedDocks = new bool[maxBotCount];
		docks = new Transform[dockHolder.childCount];
		for (int i = 0; i < dockHolder.childCount; i++)
		{
			docks[i] = dockHolder.GetChild(i);
		}
		currentHP = maxHP;
		resourceCount = UnityEngine.Random.Range(
			minLeftoverResources + botCreationCost * minInitialBotCount,
			minLeftoverResources + (botCreationCost + botUpgradeCost * maxInitialUpgrades) * maxBotCount + 1);
		inventory.AddItem(Item.Type.PureCorvorite, resourceCount);
		SpendResources();

		initialised = true;
	}

	private void Update()
	{
		if (burning)
		{

			burnTimer += Time.deltaTime;
			if (burnTimer >= burnTime)
			{
				DestroySelf(true, destroyerEntity, dropModifierOnDeath);
			}
			return;
		}
		if (enemies.Count > 0)
		{
			AlertBots(enemies);
		}
	}

	private void SpendResources(GatherBot b = null)
	{
		int usable = resourceCount - minLeftoverResources - toBeSpent;
		int creationCount = usable / botCreationCost;
		int tooMuch = Math.Max(0, creationCount + childBots.Count - maxBotCount);
		creationCount -= tooMuch;
		usable -= creationCount * botCreationCost;
		int upgradeCount = childBots.Count + creationCount < maxBotCount ? 0 : usable / botUpgradeCost;
		tooMuch = Math.Max(0, upgradeCount - (creationCount * maxInitialUpgrades + (b == null ? 0 : 1)));
		upgradeCount -= tooMuch;
		usable -= upgradeCount * botUpgradeCost;

		bool startCreationProcess = needToCreate == 0;

		//update vars
		Debug.Log(string.Format("Current resource count: {0}", resourceCount));
		Debug.Log(string.Format("Creating {0} bots at {1} each: (-{2})",
			creationCount, botCreationCost, creationCount * botCreationCost));
		needToCreate += creationCount;
		Debug.Log(string.Format("Upgrading {0} bots at {1} each: (-{2})",
			upgradeCount, botUpgradeCost, upgradeCount * botUpgradeCost));
		needToUpgrade += upgradeCount;
		toBeSpent += needToCreate * botCreationCost + needToUpgrade * botUpgradeCost;
		Debug.Log(string.Format("New resource count: {0}", resourceCount - toBeSpent));

		if (startCreationProcess)
		{
			CreationProcess(b);
		}
	}

	private void CreationProcess(GatherBot b = null)
	{
		if (b != null && needToUpgrade > 0)
		{
			UpgradeBot(b);
			needToUpgrade--;
		}

		while (needToCreate > 0)
		{
			int dockID = GetAvailableDockID();
			GatherBot newBot = CreateBot(dockID);
			needToCreate--;
			for (int i = 0; i < maxInitialUpgrades && needToUpgrade > 0; i++)
			{
				UpgradeBot(newBot);
				needToUpgrade--;
			}
			BuildBot(dockID);
		}
	}

	public void BuildBot(int dockID) => dockAnims[dockID].SetTrigger("Spawn1");

	private GatherBot CreateBot(int dockID)
	{
		if (resourceCount < botCreationCost + minLeftoverResources) return null;
		
		resourceCount -= botCreationCost;
		toBeSpent -= botCreationCost;
		inventory.RemoveItem(Item.Type.PureCorvorite, botCreationCost);
		GatherBot bot = Instantiate(botPrefab);
		bot.Create(this, botBaseHP, dockID);
		bot.Activate(false);
		occupiedDocks[dockID] = true;
		bot.transform.position = docks[dockID].position;
		bot.transform.rotation = docks[dockID].rotation;
		bot.transform.parent = transform.parent;
		childBots.Add(bot);
		AssignUnoccupiedCoords(bot);
		return bot;
	}

	private void UpgradeBot(GatherBot bot)
	{
		//bot.Upgrade();
		resourceCount -= botUpgradeCost;
		toBeSpent -= botUpgradeCost;
		inventory.RemoveItem(Item.Type.PureCorvorite, botUpgradeCost);
	}

	public void ActivateBot(int ID, Vector2 position)
	{
		for (int i = 0; i < childBots.Count; i++)
		{
			GatherBot bot = childBots[i];
			if (bot.dockID == ID)
			{
				bot.transform.position = position;
				bot.Activate(true);
				return;
			}
		}
	}

	private int GetAvailableDockID()
	{
		for (int i = 0; i < occupiedDocks.Length; i++)
		{
			if (!occupiedDocks[i]) return i;
		}
		return -1;
	}

	public Transform GetDock(GatherBot bot) => docks[bot.dockID];

	public bool IsChildBot(Entity e)
	{
		for (int i = 0; i < childBots.Count; i++)
		{
			if (childBots[i] == e) return true;
		}
		return false;
	}

	public void AssignUnoccupiedCoords(GatherBot b)
	{
		CheckEmptyMarkedCoords();
		botOccupiedCoords.Clear();
		botOccupiedCoords.AddRange(emptyCoords);

		for (int i = 0; i < childBots.Count; i++)
		{
			if (b == null) return;
			EntityNetwork.GetCoordsInRange(childBots[i].GetIntendedCoords(), 1, botOccupiedCoords);
		}

		int searchRange = 1;
		searchCoords.Clear();

		ChunkCoords location = ChunkCoords.Zero;
		bool finished = false;
		while (!finished)
		{
			if (b == null) return;
			EntityNetwork.GetCoordsOnRangeBorder(coords, searchRange, searchCoords);
			searchRange++;

			for (int i = searchCoords.Count - 1; i >= 0; i--)
			{
				ChunkCoords c = searchCoords[i];
				for (int j = 0; j < botOccupiedCoords.Count; j++)
				{
					ChunkCoords cc = botOccupiedCoords[j];
					if (c == cc)
					{
						searchCoords.RemoveAt(i);
						break;
					}
				}
			}
			if (searchCoords.Count > 0)
			{
				finished = true;
				location = searchCoords[new System.Random().Next(searchCoords.Count)];
			}
		}

		Vector2 pos = ChunkCoords.GetCenterCell(location);
		if (b == null) return;
		b.HiveOrders(pos);
	}

	public bool SplitUpGatheringUnits(GatherBot b)
	{
		for (int i = 0; i < childBots.Count; i++)
		{
			GatherBot bot = childBots[i];
			//don't check self
			if (b == bot) continue;
			//two bots should gather in different locations to avoid overcrowding
			if (b.GetIntendedCoords() == bot.GetIntendedCoords()) return true;
		}
		return false;
	}

	public bool VerifyGatheringTarget(GatherBot b, Entity e = null)
	{
		e = e == null ? b.targetEntity : e;
		for (int i = 0; i < childBots.Count; i++)
		{
			GatherBot bot = childBots[i];
			//don't check self
			if (b == bot) continue;
			//two bots shouldn't gather from the same target
			if (e == bot.targetEntity) return false;
		}
		return true;
	}

	public void BotDestroyed(GatherBot bot)
	{
		occupiedDocks[bot.dockID] = false;
		childBots.Remove(bot);
	}

	public void Store(List<ItemStack> items, GatherBot b)
	{
		b.Activate(false);
		dockAnims[b.dockID].SetTrigger("Dismantle1");
		inventory.Store(items);
		resourceCount = inventory.Count(Item.Type.PureCorvorite);
		SpendResources(b);
	}

	public override bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer,
		int dropModifier = 0, bool flash = true)
	{
		bool dead = base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
		if (dead) return CheckHealth(destroyer, dropModifier);
		ICombat threat = destroyer.GetICombat();
		if (threat != null && threat.EngageInCombat(this))
		{
			EngageInCombat(threat);
		}
		return CheckHealth(destroyer, dropModifier);
	}

	private void AlertBots(ICombat threat)
	{
		if (threat == null) return;
		for (int i = 0; i < childBots.Count; i++)
		{
			childBots[i].Alert(threat);
		}
	}

	private void AlertBots(List<ICombat> threats)
	{
		if (threats == null || threats.Count == 0) return;
		for (int i = 0; i < threats.Count; i++)
		{
			AlertBots(threats[i]);
		}
	}

	private bool CheckHealth(Entity destroyer, int dropModifier = 0)
	{
		if (currentHP > 0f) return false;
		destroyerEntity = destroyer;
		dropModifierOnDeath = dropModifier;
		burning = true;
		burningEffects.SetActive(true);
		ActivateAllColliders(false);
		sprRend.sprite = burningSprite;
		for (int i = 0; i < enemies.Count; i++)
		{
			ICombat enemy = enemies[i];
			enemy.DisengageInCombat(this);
		}
		EjectFromAllDrillers(true);
		return currentHP <= 0f;
	}

	private void DestroySelf(bool explode, Entity destroyer, int dropModifier = 0)
	{
		if (explode)
		{
			//particle effects
			GameObject explosion = Instantiate(explosionEffects, ParticleGenerator.holder, false);
			explosion.transform.position = transform.position;

			//sound effects

			//drop resources
			DropLoot(destroyer, transform.position, dropModifier);
		}

		//self destruct all child bots
		for (int i = childBots.Count - 1; i >= 0; i--)
		{
			if (childBots[i] != null)
			{
				childBots[i].DestroySelf(explode, destroyer, dropModifier);
			}
		}
		base.DestroySelf(destroyer);
	}

	private void DropLoot(Entity destroyer, Vector2 pos, int dropModifier = 0)
	{
		particleGenerator = particleGenerator ?? FindObjectOfType<ParticleGenerator>();

		for (int i = 0; i < inventory.stacks.Count; i++)
		{
			ItemStack stack = inventory.stacks[i];
			if (stack.GetItemType() == Item.Type.Blank) continue;
			particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
		}

		for (int i = 0; i < lootDrops.Count; i++)
		{
			ItemStack stack = lootDrops[i].GetStack();
			particleGenerator.DropResource(destroyer, pos, stack.GetItemType(), stack.GetAmount());
		}
	}

	public void MarkCoordAsEmpty(ChunkCoords c)
	{
		emptyCoords.Add(c);
		emptyCoordTimes.Add(TimeManager.GameTime);
	}

	private void CheckEmptyMarkedCoords()
	{
		GTime currentTime = TimeManager.GameTime;
		for (int i = 0; i < emptyCoordTimes.Count; i++)
		{
			if (GTime.SecondsBetween(emptyCoordTimes[i], currentTime) > emptyCoordWaitTime)
			{
				emptyCoords.RemoveAt(i);
				emptyCoordTimes.RemoveAt(i);
				i--;
			}
			else
			{
				return;
			}
		}
	}

	public bool IsSibling(Entity e)
	{
		for (int i = 0; i < childBots.Count; i ++)
		{
			Entity ent = childBots[i];
			if (e == ent) return true;
		}
		return false;
	}

	public override EntityType GetEntityType() => EntityType.BotHive;

	public bool EngageInCombat(ICombat hostile)
	{
		if (IsChildBot((Entity)hostile) || enemies.Contains(hostile)) return false;
		AddThreat(hostile);
		AlertBots(hostile);
		return true;
	}

	public void DisengageInCombat(ICombat nonHostile)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] == nonHostile)
			{
				enemies.RemoveAt(i);
				return;
			}
		}
	}

	private void AddThreat(ICombat threat)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			ICombat e = enemies[i];
			if (e == threat) return;
		}
		enemies.Add(threat);
	}
}