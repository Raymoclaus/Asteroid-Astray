﻿using System.Collections.Generic;
using System.Threading;
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
	private ShakeEffect shakeFX;
	[SerializeField]
	private HiveInventory inventory;

	//fields
	[SerializeField]
	private float maxHealth = 10000f, botBaseHP = 500f;
	private float currentHealth;
	private List<GatherBot> childBots = new List<GatherBot>();
	[SerializeField]
	private int minInitialBotCount = 2, maxBotCount = 4, minLeftoverResources = 1, botCreationCost = 2,
		botUpgradeCost = 4, maxInitialUpgrades = 0;
	private int resourceCount, toBeSpent;
	private bool dormant;
	private int needToCreate, needToUpgrade;
	private WaitForSeconds timeBetweenCreation = new WaitForSeconds(5f);
	private List<ChunkCoords> emptyCoords = new List<ChunkCoords>();
	private List<GTime> emptyCoordTimes = new List<GTime>();
	private float emptyCoordWaitTime = 300f;

	private void Start()
	{
		currentHealth = maxHealth;
		resourceCount = UnityEngine.Random.Range(
			minLeftoverResources + botCreationCost * minInitialBotCount,
			minLeftoverResources + (botCreationCost + botUpgradeCost * maxInitialUpgrades) * maxBotCount + 1);
		inventory.AddItem(Item.Type.Corvorite, resourceCount);
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
			StartCoroutine(CreationProcess(b));
		}
	}

	private IEnumerator CreationProcess(GatherBot b = null)
	{
		//if (b != null && needToUpgrade > 0)
		//{
		//	b.Upgrade();
		//	needToUpgrade--;
		//  toBeSpent -= botUpgradeCost;
		//  resourceCount -= botUpgradeCost;
		//}

		while (needToCreate > 0)
		{
			GatherBot newBot = CreateBot();
			needToCreate--;
			for (int i = 0; i < maxInitialUpgrades && needToUpgrade > 0; i++)
			{
				UpgradeBot(newBot);
				needToUpgrade--;
			}

			yield return timeBetweenCreation;
		}
	}

	private GatherBot CreateBot()
	{
		if (resourceCount < botCreationCost + minLeftoverResources) return null;
		
		resourceCount -= botCreationCost;
		toBeSpent -= botCreationCost;
		inventory.RemoveItem(Item.Type.Corvorite, botCreationCost);
		GatherBot bot = Instantiate(botPrefab);
		bot.transform.position = transform.position;
		bot.transform.parent = transform.parent;
		bot.Create(this, botBaseHP);
		childBots.Add(bot);
		new Thread(() =>
		{
			AssignUnoccupiedCoords(bot);
		}).Start();
		return bot;
	}

	private void UpgradeBot(GatherBot bot)
	{
		//newBot.Upgrade();
		resourceCount -= botUpgradeCost;
		toBeSpent -= botUpgradeCost;
		inventory.RemoveItem(Item.Type.Corvorite, botUpgradeCost);
	}

	public void AssignUnoccupiedCoords(GatherBot b)
	{
		CheckEmptyMarkedCoords();
		List<ChunkCoords> occupiedCoords = new List<ChunkCoords>(emptyCoords);

		foreach (GatherBot bot in childBots)
		{
			if (b == null) return;
			EntityNetwork.GetCoordsInRange(bot.GetIntendedCoords(), 1, occupiedCoords);
		}

		int searchRange = 1;
		List<ChunkCoords> searchCoords = new List<ChunkCoords>();

		ChunkCoords location = ChunkCoords.Zero;
		while (true)
		{
			if (b == null) return;
			searchCoords = EntityNetwork.GetCoordsOnRangeBorder(_coords, searchRange);
			searchRange++;
			bool finished = false;

			foreach (ChunkCoords c in searchCoords)
			{
				bool found = false;
				foreach (ChunkCoords cc in occupiedCoords)
				{
					if (cc == c)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					finished = true;
					location = c;
					break;
				}
			}

			if (finished)
			{
				break;
			}
		}

		Vector2 pos = ChunkCoords.GetCenterCell(location);
		if (b == null) return;
		b.HiveOrders(pos);
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

	public void Store(List<ItemStack> items, GatherBot b)
	{
		inventory.Store(items);
		resourceCount = inventory.Count(Item.Type.Corvorite);
		SpendResources(b);
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && !IsDrilling)
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this);
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

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0)
	{
		return TakeDamage(drillDmg, drillPos, destroyer, dropModifier);
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		shakeFX.Begin();
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		shakeFX.Stop();
	}

	public bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer, int dropModifier = 0)
	{
		currentHealth -= damage;

		return CheckHealth();
	}

	private bool CheckHealth()
	{
		return currentHealth <= 0f;
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
}