using System.Collections.Generic;
using CielaSpike;
using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HiveInventory))]
public class BotHive : Entity, IDrillableObject, IDamageable
{
	//references
	[SerializeField]
	private GatherBot botPrefab;
	[SerializeField]
	private HiveInventory inventory;
	[SerializeField]
	private Transform dockHolder;
	private Transform[] docks;
	[SerializeField]
	private Animator[] dockAnims;
	[SerializeField]
	private AudioSO collisionSounds;

	//fields
	[SerializeField]
	private float maxHealth = 10000f, botBaseHP = 500f;
	private float currentHealth;
	[HideInInspector]
	public List<GatherBot> childBots = new List<GatherBot>();
	private bool[] occupiedDocks;
	[SerializeField]
	private int minInitialBotCount = 2, maxBotCount = 3, minLeftoverResources = 1, botCreationCost = 2,
		botUpgradeCost = 4, maxInitialUpgrades = 0;
	private int resourceCount, toBeSpent;
	private bool dormant;
	private int needToCreate, needToUpgrade;
	private List<ChunkCoords> emptyCoords = new List<ChunkCoords>();
	private List<GTime> emptyCoordTimes = new List<GTime>();
	private float emptyCoordWaitTime = 300f;
	public WaitForSeconds maintenanceTime = new WaitForSeconds(3f);

	//cache
	private List<ChunkCoords> botOccupiedCoords = new List<ChunkCoords>();
	private List<ChunkCoords> searchCoords = new List<ChunkCoords>();
	private ContactPoint2D[] contacts = new ContactPoint2D[1];

	private void Start()
	{
		occupiedDocks = new bool[maxBotCount];
		docks = new Transform[dockHolder.childCount];
		for (int i = 0; i < dockHolder.childCount; i++)
		{
			docks[i] = dockHolder.GetChild(i);
		}
		currentHealth = maxHealth;
		resourceCount = UnityEngine.Random.Range(
			minLeftoverResources + botCreationCost * minInitialBotCount,
			minLeftoverResources + (botCreationCost + botUpgradeCost * maxInitialUpgrades) * maxBotCount + 1);
		inventory.AddItem(Item.Type.PureCorvorite, resourceCount);
		SpendResources();

		initialised = true;
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

	public void BuildBot(int dockID)
	{
		dockAnims[dockID].SetTrigger("Spawn1");
	}

	private GatherBot CreateBot(int dockID)
	{
		if (resourceCount < botCreationCost + minLeftoverResources) return null;
		
		resourceCount -= botCreationCost;
		toBeSpent -= botCreationCost;
		inventory.RemoveItem(Item.Type.PureCorvorite, botCreationCost);
		GatherBot bot = Instantiate(botPrefab);
		bot.gameObject.SetActive(false);
		bot.Create(this, botBaseHP, dockID);
		occupiedDocks[dockID] = true;
		bot.transform.position = docks[dockID].position;
		bot.transform.rotation = docks[dockID].rotation;
		bot.transform.parent = transform.parent;
		childBots.Add(bot);
		this.StartCoroutineAsync(AssignUnoccupiedCoords(bot));
		return bot;
	}

	private void UpgradeBot(GatherBot bot)
	{
		//bot.Upgrade();
		resourceCount -= botUpgradeCost;
		toBeSpent -= botUpgradeCost;
		inventory.RemoveItem(Item.Type.PureCorvorite, botUpgradeCost);
	}

	public IEnumerator ActivateBot(int ID, Vector2 position)
	{
		yield return null;
		foreach (GatherBot bot in childBots)
		{
			if (bot.dockID == ID)
			{
				bot.transform.position = position;
				bot.gameObject.SetActive(true);
				yield break;
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

	public Transform GetDock(GatherBot bot)
	{
		return docks[bot.dockID];
	}

	public IEnumerator AssignUnoccupiedCoords(GatherBot b)
	{
		Debug.Log($"bot {b.dockID}: Looking for coordinates.");
		CheckEmptyMarkedCoords();
		botOccupiedCoords.Clear();
		botOccupiedCoords.AddRange(emptyCoords);

		for (int i = 0; i < childBots.Count; i++)
		{
			if (b == null) yield break; ;
			EntityNetwork.GetCoordsInRange(childBots[i].GetIntendedCoords(), 1, botOccupiedCoords);
		}

		int searchRange = 1;
		searchCoords.Clear();

		ChunkCoords location = ChunkCoords.Zero;
		bool finished = false;
		while (!finished)
		{
			if (b == null) yield break;
			EntityNetwork.GetCoordsOnRangeBorder(_coords, searchRange, searchCoords);
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
			yield return null;
		}

		Vector2 pos = ChunkCoords.GetCenterCell(location);
		if (b == null) yield break;
		b.HiveOrders(pos);
		Debug.Log($"bot {b.dockID}: Found coordinates.");
	}

	public bool SplitUpGatheringUnits(GatherBot b)
	{
		foreach (GatherBot bot in childBots)
		{
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
		foreach (GatherBot bot in childBots)
		{
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
		b.gameObject.SetActive(false);
		dockAnims[b.dockID].SetTrigger("Dismantle1");
		inventory.Store(items);
		resourceCount = inventory.Count(Item.Type.PureCorvorite);
		SpendResources(b);
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill && IsDrillable())
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && !IsDrilling)
			{
				StartDrilling();
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
				otherDrill.StopDrilling();
			}
		}
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		Collider2D other = collision.collider;
		int otherLayer = other.gameObject.layer;
		collision.GetContacts(contacts);
		Vector2 contactPoint = contacts[0].point;

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
		}
	}

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0)
	{
		return TakeDamage(drillDmg, drillPos, destroyer, dropModifier);
	}

	public void StartDrilling()
	{

	}

	public void StopDrilling()
	{

	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0)
	{
		currentHealth -= damage;

		return CheckHealth(destroyer, dropModifier);
	}

	private bool CheckHealth(Entity destroyer, int dropModifier = 0)
	{
		if (currentHealth > 0f) return false;
		DestroySelf(true, destroyer, dropModifier);
		return currentHealth <= 0f;
	}

	private void DestroySelf(bool explode, Entity destroyer, int dropModifier = 0)
	{
		if (explode)
		{
			//particle effects

			//sound effects

			//drop resources

		}

		//self destruct all child bots
		for (int i = childBots.Count - 1; i >= 0; i--)
		{
			if (childBots[i] != null)
			{
				childBots[i].DestroySelf(explode, destroyer, dropModifier);
			}
		}
		destroyer.DestroyedAnEntity(this);
		base.DestroySelf();
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

	public Vector2 GetPosition()
	{
		return transform.position;
	}

	public bool IsSibling(Entity e)
	{
		foreach (Entity ent in childBots)
		{
			if (e == ent) return true;
		}
		return false;
	}

	public override EntityType GetEntityType()
	{
		return EntityType.BotHive;
	}

	public void Launch(Vector2 launchDirection, Entity launcher)
	{

	}

	public bool IsDrillable()
	{
		return true;
	}
}