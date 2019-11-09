using System.Collections.Generic;
using System;
using UnityEngine;
using InventorySystem;
using CustomDataTypes;
using AudioUtilities;

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
	[SerializeField] private float burnTime = 3f;
	private float burnTimer = 0f;
	private bool burning = false;
	private Entity destroyerEntity;
	private float dropModifierOnDeath;

	//cache
	private List<ChunkCoords> botOccupiedCoords = new List<ChunkCoords>();
	private List<ChunkCoords> searchCoords = new List<ChunkCoords>();
	#endregion

	protected override void Awake()
	{
		base.Awake();
		docks = new Transform[dockHolder.childCount];
		for (int i = 0; i < dockHolder.childCount; i++)
		{
			docks[i] = dockHolder.GetChild(i);
		}
	}

	private void Start()
	{
		resourceCount = UnityEngine.Random.Range(
			minLeftoverResources + botCreationCost * minInitialBotCount,
			minLeftoverResources + (botCreationCost + botUpgradeCost * maxInitialUpgrades) * maxBotCount + 1);
		inventory.AddItem(Item.Type.PureCorvorite, resourceCount);
		SpendResources();
	}

	protected override void Update()
	{
		base.Update();
		if (burning)
		{
			burnTimer += Time.deltaTime;
			if (burnTimer >= burnTime)
			{
				DestroySelf(destroyerEntity, dropModifierOnDeath);
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
		}
	}

	public void BuildBot(int dockID) => dockAnims[dockID].SetTrigger("Spawn1");

	private GatherBot CreateBot(int dockID, bool ignoreCost = false)
	{
		if (!ignoreCost)
		{
			if (resourceCount < botCreationCost + minLeftoverResources) return null;

			resourceCount -= botCreationCost;
			toBeSpent -= botCreationCost;
			inventory.RemoveItem(Item.Type.PureCorvorite, botCreationCost);
		}
		GatherBot bot = Instantiate(botPrefab);
		bot.Create(this, botBaseHP, dockID);
		bot.Activate(false);
		occupiedDocks[dockID] = true;
		bot.transform.position = docks[dockID].position;
		bot.transform.rotation = docks[dockID].rotation;
		bot.transform.parent = transform.parent;
		childBots.Add(bot);
		AssignUnoccupiedCoords(bot);
		BuildBot(dockID);
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
		if (occupiedDocks == null)
		{
			occupiedDocks = new bool[maxBotCount];
		}
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

		Vector2 pos = ChunkCoords.GetCenterCell(location, EntityNetwork.CHUNK_SIZE);
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

	public override bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		bool dead = base.TakeDamage(damage, damagePos, destroyer, dropModifier, flash);
		if (dead) return true;
		ICombat threat = destroyer.GetICombat();
		if (threat != null && threat.EngageInCombat(this))
		{
			EngageInCombat(threat);
		}
		return false;
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

	protected override bool CheckHealth(Entity destroyer, float dropModifier)
	{
		if (HealthRatio > 0f) return false;
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
		return base.CheckHealth(destroyer, dropModifier);
	}

	public override void DestroySelf(Entity destroyer, float dropModifier)
	{
		bool explode = destroyer != null;
		if (explode)
		{
			//particle effects
			GameObject explosion = Instantiate(explosionEffects, ParticleGenerator.holder, false);
			explosion.transform.position = transform.position;

			//sound effects
		}

		//self destruct all child bots
		for (int i = childBots.Count - 1; i >= 0; i--)
		{
			if (childBots[i] != null)
			{
				childBots[i].DestroySelf(destroyer, dropModifier);
			}
		}
		base.DestroySelf(destroyer, dropModifier);
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

	protected override object CreateDataObject()
	{
		List<GatherBot.GatherBotData> botData = new List<GatherBot.GatherBotData>();
		for (int i = 0; i < childBots.Count; i++)
		{
			botData.Add(childBots[i].GetData());
		}
		return new HiveSaveData(transform.position, DefaultInventory.GetInventoryData(), botData);
	}

	public override void ApplyData(EntityData? data)
	{
		if (data == null) return;
		HiveSaveData d = (HiveSaveData)((EntityData)data).data;
		transform.position = d.position;
		DefaultInventory.SetData(d.inventory);
		for (int i = 0; i < d.botData.Count; i++)
		{
			CreateBot(GetAvailableDockID(), true).ApplyData(d.botData[i]);
		}
	}

	[Serializable]
	private struct HiveSaveData
	{
		public SerializableVector3 position;
		public Storage.InventoryData inventory;
		public List<GatherBot.GatherBotData> botData;

		public HiveSaveData(SerializableVector3 position, Storage.InventoryData inventory,
			List<GatherBot.GatherBotData> botData)
		{
			this.position = position;
			this.inventory = inventory;
			this.botData = botData;
		}
	}
}