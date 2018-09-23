using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity Fields")]
	[SerializeField]
	protected ChunkCoords _coords;
	public Collider2D[] Col;
	public Rigidbody2D Rb;
	public bool ShouldDisablePhysicsOnDistance = true;
	public bool ShouldDisableObjectOnDistance = true;
	public bool ShouldDisableGameObjectOnShortDistance = true;
	public bool isActive = true;
	public bool disabled = false;
	public bool isInPhysicsRange = false;
	private Vector3 vel;
	private float disableTime;
	protected bool needsInit = true;
	protected bool initialised = false;

	//related layers
	private static bool layersSet;
	protected static int layerDrill, layerProjectile, layerSolid;

	//drill related
	public bool canDrill;
	protected DrillBit drill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer[] rends;

	private static int entitiesActive;

	public static int GetActive()
	{
		return entitiesActive;
	}

	public virtual void Awake()
	{
		entitiesActive++;
		_coords = new ChunkCoords(transform.position);
		EntityNetwork.AddEntity(this, _coords);
		GetLayers();
	}

	public virtual void LateUpdate()
	{
		RepositionInNetwork();
	}

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		bool repositioned = false;
		if (newCc != _coords)
		{ 
			EntityNetwork.Reposition(this, newCc);
			repositioned = true;
		}

		if (needsInit && !initialised) return;

		SetAllActivity(IsInView());
		isInPhysicsRange = IsInPhysicsRange();
		if (ShouldDisablePhysicsOnDistance)
		{
			if (isInPhysicsRange)
			{
				if (!disabled) return;
				entitiesActive++;
				disabled = false;
				gameObject.SetActive(true);
				if (Rb != null)
				{
					Rb.simulated = true;
				}
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				if (repositioned && OnExitPhysicsRange()) return;
				entitiesActive--;
				disabled = true;
				vel = Rb == null ? vel : (Vector3)Rb.velocity;
				if (Rb != null)
				{
					Rb.simulated = !ShouldDisablePhysicsOnDistance;
				}
				gameObject.SetActive(!ShouldDisablePhysicsOnDistance);
			}
		}
	}

	public virtual bool OnExitPhysicsRange()
	{
		return false;
	}

	public void SetCoordinates(ChunkCoords newCc)
	{
		_coords = newCc;
	}

	protected bool IsInView()
	{
		return CameraCtrl.IsCoordInView(_coords);
	}

	protected bool IsInPhysicsRange()
	{
		return CameraCtrl.IsCoordInPhysicsRange(_coords);
	}

	public void SetAllActivity(bool active)
	{
		if (active == isActive || !ShouldDisableObjectOnDistance) return;
		if (needsInit && !initialised) return;

		isActive = active;

		if (ShouldDisableGameObjectOnShortDistance)
		{
			gameObject.SetActive(active);
			return;
		}

		foreach (Renderer r in rends)
		{
			if (r != null)
			{
				r.enabled = active;
			}
		}

		//enable/disable all relevant components
		foreach (MonoBehaviour script in ScriptComponents)
		{
			if (script != null)
			{
				script.enabled = active;
			}
		}
	}

	public virtual void CollectResources(ResourceDrop r)
	{

	}

	public virtual EntityType GetEntityType()
	{
		return EntityType.Entity;
	}

	public virtual void DestroySelf()
	{
		if (EntityNetwork.ConfirmLocation(this, _coords))
		{
			EntityNetwork.RemoveEntity(this);
		}
		Destroy(gameObject);
	}

	public ChunkCoords GetCoords()
	{
		return _coords;
	}

	public override string ToString()
	{
		return string.Format("{0} at coordinates {1}.", GetEntityType(), _coords);
	}

	public DrillBit GetDrill()
	{
		return canDrill ? drill : null;
	}

	public void SetDrill(DrillBit newDrill)
	{
		canDrill = newDrill != null;
		drill = newDrill;
	}

	public void AttachDrill(DrillBit db)
	{
		drill = db;
	}

	//This should be overridden. Called by a drill to determine how much damage it should deal to its target.
	public virtual float DrillDamageQuery(bool firstHit)
	{
		return 1f;
	}

	//This should be overridden. Called by a drill to alert the entity that the drilling has completed
	public virtual void DrillComplete()
	{

	}

	//some entities might want to avoid drilling other entities by accident, override to verify target
	public virtual bool VerifyDrillTarget(Entity target)
	{
		return true;
	}

	public virtual void PhysicsReEnabled()
	{

	}

	private void GetLayers()
	{
		if (layersSet) return;

		layerDrill = LayerMask.NameToLayer("Drill");
		layerProjectile = LayerMask.NameToLayer("Projectile");
		layerSolid = LayerMask.NameToLayer("Solid");

		layersSet = true;
	}

	public virtual Scan ReturnScan()
	{
		return new Scan(GetEntityType(), 1f, 1);
	}

	public virtual bool CanFireLaser()
	{
		return false;
	}

	public virtual void DestroyedAnEntity(Entity target)
	{

	}
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