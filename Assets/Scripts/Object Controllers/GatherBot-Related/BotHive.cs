using System.Collections.Generic;
using UnityEngine;

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
	private int minInitialBotCount = 2, maxBotCount = 4, minLeftoverResources = 1, botCreationCost = 3,
		botUpgradeCost = 2, maxInitialUpgrades = 1;
	private int resourceCount;
	private bool dormant;

	private void Start()
	{
		resourceCount = Random.Range(
			minLeftoverResources + botCreationCost * minInitialBotCount,
			minLeftoverResources + (botCreationCost + botUpgradeCost * maxInitialUpgrades) * maxBotCount + 1);
		inventory.AddItem(Item.Type.Corvorite, resourceCount);
		SpendResources();

		initialised = true;
	}

	private void SpendResources()
	{
		List<GatherBot> newBots = new List<GatherBot>();

		//create bots
		while (childBots.Count < maxBotCount)
		{
			GatherBot bot = CreateBot();
			if (bot == null)
			{
				break;
			}
			else
			{
				newBots.Add(bot);
			}
		}

		//upgrade bots
		//int i = 0;
		//while (resourceCount >= botUpgradeCost + minLeftoverResources)
		//{
		//	newBots[i].Upgrade();
		//	resourceCount -= botUpgradeCost;
		//	i++;
		//}
	}

	private GatherBot CreateBot()
	{
		if (resourceCount < botCreationCost + minLeftoverResources) return null;

		resourceCount -= botCreationCost;
		GatherBot bot = Instantiate(botPrefab);
		bot.transform.position = transform.position;
		bot.Create(this, botBaseHP);
		childBots.Add(bot);

		return bot;
	}

	public void Store(List<ItemStack> items)
	{
		inventory.Store(items);
		resourceCount = inventory.Count(Item.Type.Corvorite);
		SpendResources();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill)
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

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos)
	{
		return TakeDamage(drillDmg, drillPos);
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

	public bool TakeDamage(float damage, Vector2 damagePos)
	{
		currentHealth -= damage;

		return CheckHealth();
	}

	private bool CheckHealth()
	{
		return currentHealth <= 0f;
	}
}