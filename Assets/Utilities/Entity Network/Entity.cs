using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;
using InventorySystem;
using ValueComponents;
using TriggerSystem;
using AttackData;
using AudioUtilities;

public class Entity : MonoBehaviour, IActionMessageReceiver, IAttackMessageReceiver
{
	[Header("Entity Fields")]
	[SerializeField] protected ChunkCoords coords;
	public Collider2D[] col;
	public Rigidbody2D rb;
	private static Camera mainCam;
	protected static Camera MainCam { get { return mainCam ?? (mainCam = Camera.main); } }
	private static CameraCtrl mainCamCtrl;
	protected static CameraCtrl CameraControl
	{
		get { return mainCamCtrl ?? (mainCamCtrl = MainCam.GetComponent<CameraCtrl>()); }
	}
	[SerializeField] private static ParticleGenerator partGen;
	protected static ParticleGenerator PartGen
		=> partGen ?? (partGen = FindObjectOfType<ParticleGenerator>());
	protected static AudioManager audioMngr;
	protected static AudioManager AudioMngr
		=> audioMngr ?? (audioMngr = FindObjectOfType<AudioManager>());

	[SerializeField] protected ScreenRippleEffectController screenRippleSO;
	[SerializeField] private bool shouldDisablePhysicsOnDistance = true;
	[SerializeField] private bool shouldDisableObjectOnDistance = true;
	[SerializeField] private bool shouldDisableGameObjectOnShortDistance = true;
	[HideInInspector] public bool isActive = true;
	[HideInInspector] public bool disabled = false;
	[HideInInspector] public bool isInPhysicsRange = false;
	private Vector3 vel;
	private float disableTime;
	[SerializeField] protected bool isInvulnerable;

	[SerializeField] protected RangedFloatComponent healthComponent;
	[SerializeField] private LootComponent loot;

	//layers
	protected static int layerDrill = -1,
		layerProjectile = -1,
		layerSolid = -1,
		layerAttack = -1,
		layerShield = -1;
	protected static int LayerDrill => layerDrill >= 0
		? layerDrill
		: (layerDrill = LayerMask.NameToLayer("Drill"));
	protected static int LayerProjectile => layerProjectile >= 0
		? layerProjectile
		: (layerProjectile = LayerMask.NameToLayer("Projectile"));
	protected static int LayerSolid => layerSolid >= 0
		? layerSolid
		: (layerSolid = LayerMask.NameToLayer("Solid"));
	protected static int LayerAttack => layerAttack >= 0
		? layerAttack
		: (layerAttack = LayerMask.NameToLayer("Attack"));
	protected static int LayerShield => layerShield >= 0
		? layerShield
		: (layerShield = LayerMask.NameToLayer("Shield"));

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer[] rends;

	private static int entitiesActive;

	protected virtual void Awake()
	{
		enabled = false;
		LoadingController.AddListener(Initialise);
	}

	public virtual void Initialise()
	{
		entitiesActive++;
		coords = new ChunkCoords(transform.position, EntityNetwork.CHUNK_SIZE);
		EntityNetwork.AddEntity(this, coords);
		healthComponent?.SetToUpperLimit();
		enabled = true;
	}

	public virtual void LateUpdate() => RepositionInNetwork();

	private void OnDestroy()
	{
		EntityNetwork.RemoveEntity(this);
		mainCam = null;
		mainCamCtrl = null;
	}

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position, EntityNetwork.CHUNK_SIZE);
		bool repositioned = false;
		if (newCc != coords)
		{ 
			EntityNetwork.Reposition(this, newCc);
			repositioned = true;
		}

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

	protected float DistanceFromCenter => transform.position.magnitude;

	public virtual bool OnExitPhysicsRange() => false;

	public void SetCoordinates(ChunkCoords newCc) => coords = newCc;

	protected bool IsInView() => CameraControl?.IsCoordInView(coords) ?? false;

	protected bool IsInPhysicsRange() => CameraControl?.IsCoordInPhysicsRange(coords) ?? false;

	public void SetAllActivity(bool active)
	{
		if (active == isActive || !shouldDisableObjectOnDistance) return;

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

	public float HealthRatio => healthComponent.CurrentRatio;

	public virtual ICombat GetICombat() => null;

	public virtual EntityType GetEntityType() => EntityType.Entity;

	public virtual void DestroySelf(Entity destroyer, float dropModifier)
	{
		if (destroyer)
		{
			destroyer.DestroyedAnEntity(this);
		}
		if (EntityNetwork.ConfirmLocation(this, coords))
		{
			EntityNetwork.RemoveEntity(this);
		}
		IInventoryHolder target = destroyer as IInventoryHolder;
		DropLoot(target, dropModifier);
		Destroy(gameObject);
	}

	protected virtual bool CheckHealth(Entity destroyer, float dropModifier)
	{
		if (healthComponent.CurrentRatio > 0f) return false;
		DestroySelf(destroyer, dropModifier);
		return true;
	}

	protected virtual void DropLoot(IInventoryHolder target, float dropModifier)
	{
		if (loot == null) return;
		loot.DropAllLoot(target);
	}

	public ChunkCoords GetCoords() => coords;

	public override string ToString() => string.Format("{0} at coordinates {1}.", GetEntityType(), coords);

	public virtual void PhysicsReEnabled() { }

	public virtual Scan ReturnScan() => new Scan(GetEntityType(), healthComponent.CurrentRatio, GetLevel(), GetValue()); 

	protected virtual int GetLevel() => 1;

	protected virtual int GetValue() => 0;

	public virtual void DestroyedAnEntity(Entity target) { }

	public virtual void Launching() { }

	public virtual bool CanFireLaser() => false;

	public virtual bool CanFireStraightWeapon() => false;

	public virtual void AttachLaser(bool attach) { }

	public virtual void AttachStraightWeapon(bool attach) { }

	public static int GetActive() => entitiesActive;

	protected virtual void OnCollisionEnter2D(Collision2D collision)
	{
		Collider2D other = collision.collider;
		int otherLayer = other.gameObject.layer;
		//collision.GetContacts(contacts);
		//Vector2 contactPoint = contacts[0].point;
		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;
		float angle = -Vector2.SignedAngle(Vector2.up, contactPoint - (Vector2)transform.position);

		if (otherLayer == LayerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this, contactPoint);
		}

		if (otherLayer == LayerSolid)
		{
			if (launched)
			{
				LaunchImpact(angle, contactPoint, other);
			}
		}
	}

	public virtual bool TakeDamage(float damage, Vector2 damagePos,
		Entity destroyer, float dropModifier, bool flash)
	{
		if (destroyer == this || isInvulnerable) return false;

		healthComponent.SubtractValue(damage);
		return CheckHealth(destroyer, dropModifier);
	}

	public void Teleport(Vector2 position) => transform.position = position;

	protected virtual object CreateDataObject() => null;

	public virtual void ApplyData(EntityData? data) { }
	
	[Header("Launch related variables")]
	private Character launcher;
	protected bool launched;
	private LaunchTrailController launchTrail;
	[SerializeField] protected float launchDuration = 2f;
	[SerializeField] protected float launchTrailScale = 1f;
	[SerializeField] protected bool isLaunchable = true;

	public virtual bool CanBeLaunched() => isLaunchable;

	public virtual void Launch(Vector2 launchDirection, Character launcher)
	{
		if (!isLaunchable) return;

		this.launcher = launcher;
		rb.velocity = launchDirection;
		launched = true;
		CameraControl?.Pan(transform);
		if (launcher.GetLaunchTrailAnimation() != null)
		{
			launchTrail = Instantiate(launcher.GetLaunchTrailAnimation());
			launchTrail.SetFollowTarget(transform, launchDirection, launchTrailScale);
		}

		Pause.DelayedAction(() =>
		{
			launchTrail?.EndLaunchTrail();

			this.launcher = null;
			launched = false;
			if (this == null) return;
			if (CameraControl?.GetPanTarget() == transform)
			{
				CameraControl?.Pan(null);
			}
		}, launchDuration, true);
	}

	protected virtual void LaunchImpact(float angle, Vector2 contactPoint, Collider2D other)
	{
		if (launcher.GetLaunchImpactAnimation() != null)
		{
			Transform impact = Instantiate(launcher.GetLaunchImpactAnimation()).transform;
			impact.parent = ParticleGenerator.holder;
			impact.position = contactPoint;
			impact.eulerAngles = Vector3.forward * angle;
		}
		if (launchTrail != null)
		{
			launchTrail.CutLaunchTrail();
		}
		Entity otherDamageable = other.attachedRigidbody.gameObject.GetComponent<Entity>();
		float damage = launcher.GetLaunchDamage();
		if (healthComponent.CurrentRatio < 0.5)
		{
			damage *= 2f;
		}
		otherDamageable?.TakeDamage(damage, contactPoint, launcher, 1f, true);
		TakeDamage(damage, contactPoint, launcher, 1f, true);
		launched = false;
		if (CameraControl?.GetPanTarget() == transform)
		{
			CameraControl.Pan(null);
		}
	}
	
	[Header("Drill related variables")]
	private List<DrillBit> drillers = new List<DrillBit>();
	[SerializeField] private bool isDrillable = true;

	public virtual bool TakeDrillDamage(float drillDmg, Vector2 drillPos,
		Entity destroyer, float dropModifier)
		=> TakeDamage(drillDmg, drillPos, destroyer, dropModifier, true);

	public virtual void StartDrilling(DrillBit db)
	{
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
		AddDriller(db);
	}

	public virtual void StopDrilling(DrillBit db)
	{
		rb.constraints = RigidbodyConstraints2D.None;
		RemoveDriller(db);
	}

	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == LayerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill && otherDrill.drillTarget == null && otherDrill.Verify(this))
			{
				StartDrilling(otherDrill);
				otherDrill.StartDrilling(this);
			}
		}
	}

	protected virtual void OnTriggerExit2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == LayerDrill && IsDrillable())
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();

			if (otherDrill.drillTarget == this)
			{
				StopDrilling(otherDrill);
				otherDrill.StopDrilling(false);
			}
		}
	}

	public virtual bool IsDrillable() => isDrillable;

	private List<DrillBit> GetDrillers() => drillers;

	private void AddDriller(DrillBit db) => GetDrillers().Add(db);

	private bool RemoveDriller(DrillBit db)
	{
		List<DrillBit> drills = GetDrillers();
		for (int i = 0; i < drills.Count; i++)
		{
			if (drills[i] == db)
			{
				drills.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	protected void EjectFromAllDrillers(bool successful)
	{
		List<DrillBit> drills = GetDrillers();
		for (int i = drills.Count - 1; i >= 0; i--)
		{
			drills[i].StopDrilling(successful);
		}
	}

	public void Interacted(IInteractor interactor, string action)
	{
		interactor.Interact(this);
	}

	public virtual bool ReceiveAttack(AttackManager atkMngr)
	{
		if (atkMngr == null) return false;
		OwnerComponent ownerComponent = atkMngr.GetAttackComponent<OwnerComponent>();
		List<IAttacker> owners = ownerComponent.owners;
		IAttacker thisAttacker = (this as IAttacker);
		Entity destroyer = null;
		foreach (IAttacker owner in owners)
		{
			if (owner is Entity e)
			{
				destroyer = e;
				break;
			}
		}
		if (thisAttacker != null && owners.Contains(thisAttacker)) return false;
		object damageObj = atkMngr.GetData<DamageComponent>();
		if (damageObj != null)
		{
			float damage = (float) damageObj;
			TakeDamage(damage, atkMngr.Position, destroyer, 1f, true);
		}

		return true;
	}

	public virtual bool CanReceiveAttackMessagesFromLayer(int layer)
		=> layer == LayerSolid
		   || layer == LayerShield;
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle,
	Nebula,
	BotHive,
	GatherBot,
	Planet
}

[System.Serializable]
public struct EntityData
{
	public System.Type type;
	public object data;

	public EntityData(System.Type type, object data)
	{
		this.type = type;
		this.data = data;
	}

	public override string ToString()
	{
		return type.ToString();
	}
}