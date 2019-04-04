using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity Fields")]
	[SerializeField] protected ChunkCoords coords;
	public Collider2D[] col;
	public Rigidbody2D rb;
	[SerializeField] protected CameraCtrlTracker camTrackerSO;
	[SerializeField] protected static ParticleGenerator particleGenerator;
	protected static AudioManager audioManager;
	[SerializeField] protected ScreenRippleEffectController screenRippleSO;
	protected bool entityReady = false;
	[SerializeField] private bool shouldDisablePhysicsOnDistance = true;
	[SerializeField] private bool shouldDisableObjectOnDistance = true;
	[SerializeField] private bool shouldDisableGameObjectOnShortDistance = true;
	[HideInInspector] public bool isActive = true;
	[HideInInspector] public bool disabled = false;
	[HideInInspector] public bool isInPhysicsRange = false;
	private Vector3 vel;
	private float disableTime;
	protected bool needsInit = true;
	protected bool initialised = false;

	[SerializeField] protected float maxHP = 1000f;
	protected float currentHP;

	//related layers
	private static bool layersSet;
	protected static int layerDrill, layerProjectile, layerSolid;

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer[] rends;

	public delegate void HealthUpdateHandler(float oldVal, float newVal);
	public event HealthUpdateHandler OnHealthUpdate;
	protected void HealthUpdated(float oldVal, float newVal) => OnHealthUpdate?.Invoke(oldVal, newVal);

	private static int entitiesActive;

	public virtual void Awake()
	{
		currentHP = maxHP;
		if (!EntityNetwork.ready)
		{
			gameObject.SetActive(false);
			EntityNetwork.postInitActions.Add(() =>
			{
				Initialise();
				gameObject.SetActive(true);
			});
		}
		else
		{
			Initialise();
		}
	}

	public virtual void Initialise()
	{
		entitiesActive++;
		coords = new ChunkCoords(transform.position);
		EntityNetwork.AddEntity(this, coords);
		GetLayers();
		entityReady = true;
	}

	public virtual void LateUpdate() => RepositionInNetwork();

	private void OnDestroy() => EntityNetwork.RemoveEntity(this);

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		bool repositioned = false;
		if (newCc != coords)
		{ 
			EntityNetwork.Reposition(this, newCc);
			repositioned = true;
		}

		if (needsInit && !initialised) return;

		SetAllActivity(IsInView());
		isInPhysicsRange = IsInPhysicsRange();
		if (shouldDisablePhysicsOnDistance)
		{
			if (isInPhysicsRange)
			{
				if (!disabled) return;
				entitiesActive++;
				disabled = false;
				gameObject.SetActive(true);
				if (rb != null)
				{
					rb.simulated = true;
				}
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				if (repositioned && OnExitPhysicsRange()) return;
				entitiesActive--;
				disabled = true;
				vel = rb == null ? vel : (Vector3)rb.velocity;
				if (rb != null)
				{
					rb.simulated = !shouldDisablePhysicsOnDistance;
				}
				gameObject.SetActive(!shouldDisablePhysicsOnDistance);
			}
		}
	}

	public virtual bool OnExitPhysicsRange() => false;

	public void SetCoordinates(ChunkCoords newCc) => coords = newCc;

	protected bool IsInView() => camTrackerSO?.IsCoordInView(coords) ?? false;

	protected bool IsInPhysicsRange() => camTrackerSO?.IsCoordInPhysicsRange(coords) ?? false;

	public void SetAllActivity(bool active)
	{
		if (active == isActive || !shouldDisableObjectOnDistance) return;
		if (needsInit && !initialised) return;

		isActive = active;

		if (shouldDisableGameObjectOnShortDistance)
		{
			gameObject.SetActive(active);
			return;
		}

		ActivateRenderers(active);

		//enable/disable all relevant components
		for (int i = 0; i < ScriptComponents.Count; i++)
		{
			MonoBehaviour script = ScriptComponents[i];
			if (script != null)
			{
				script.enabled = active;
			}
		}
	}

	protected void ActivateAllColliders(bool activate)
	{
		for (int i = 0; i < col.Length; i++)
		{
			col[i].enabled = activate;
		}
	}

	protected void ActivateRenderers(bool active)
	{
		if (active) active = ShouldBeVisible();

		for (int i = 0; i < rends.Length; i++)
		{
			Renderer r = rends[i];
			if (r != null) r.enabled = active;
		}
	}

	protected virtual bool ShouldBeVisible() => true;

	public virtual ICombat GetICombat() => null;

	public virtual void CollectResources(Item.Type type, int amount) { }

	public virtual EntityType GetEntityType() => EntityType.Entity;

	public virtual void DestroySelf(Entity destroyer)
	{
		if (destroyer)
		{
			destroyer.DestroyedAnEntity(this);
		}
		if (EntityNetwork.ConfirmLocation(this, coords))
		{
			EntityNetwork.RemoveEntity(this);
		}
		Destroy(gameObject);
	}

	public ChunkCoords GetCoords() => coords;

	public override string ToString() => string.Format("{0} at coordinates {1}.", GetEntityType(), coords);

	public virtual LaunchTrailController GetLaunchTrailAnimation() => null;

	public virtual GameObject GetLaunchImpactAnimation() => null;

	public virtual void PhysicsReEnabled() { }

	private void GetLayers()
	{
		if (layersSet) return;

		layerDrill = LayerMask.NameToLayer("Drill");
		layerProjectile = LayerMask.NameToLayer("Projectile");
		layerSolid = LayerMask.NameToLayer("Solid");

		layersSet = true;
	}

	public virtual Scan ReturnScan() => new Scan(GetEntityType(), GetHpRatio(), GetLevel(), GetValue()); 

	public float GetHpRatio() => currentHP / maxHP;

	protected virtual int GetLevel() => 1;

	protected virtual int GetValue() => 0;

	public virtual void DestroyedAnEntity(Entity target) { }

	public virtual void Launching() { }

	public virtual bool CanFireLaser() => false;

	public virtual bool CanFireStraightWeapon() => false;

	public virtual void AttachLaser(bool attach) { }

	public virtual void AttachStraightWeapon(bool attach) { }

	public static int GetActive() => entitiesActive;

	public virtual bool TakeDamage(float damage, Vector2 damagePos, Entity destroyer,
		int dropModifier = 0, bool flash = true) => false;

	public void Teleport(Vector2 position) => transform.position = position;
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle,
	Nebula,
	BotHive,
	GatherBot
}