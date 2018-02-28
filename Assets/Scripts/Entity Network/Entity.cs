using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity Fields")]
	protected ChunkCoords _coords;
	public Collider2D Col;
	public Rigidbody2D Rb;
	public bool ShouldDisablePhysicsOnDistance = true;
	public bool ShouldDisableObjectOnDistance = true;
	private bool _isActive = true;
	private bool disabled = false;
	private Vector3 vel;
	private float disableTime;
	protected bool needsInit = false;
	protected bool initialised = false;

	//drill related
	public bool canDrill;
	private DrillBit drill;
	protected LayerMask layerDrill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer Rend;
	public Animator Anim;

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

		layerDrill = LayerMask.NameToLayer("Drill");
	}

	public virtual void LateUpdate()
	{
		RepositionInNetwork();
	}

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		if (newCc != _coords)
			EntityNetwork.Reposition(this, newCc);
		SetAllActivity(IsInView());
		if (ShouldDisablePhysicsOnDistance)
		{
			if (IsInPhysicsRange())
			{
				if (!disabled) return;
				entitiesActive++;
				disabled = false;
				gameObject.SetActive(true);
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				entitiesActive--;
				disabled = true;
				vel = Rb == null ? vel : (Vector3)Rb.velocity;
				gameObject.SetActive(!ShouldDisableObjectOnDistance);
			}
		}
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

	public virtual void SetAllActivity(bool active)
	{
		if (needsInit && !initialised) return;

		if (active == _isActive || !ShouldDisableObjectOnDistance) return;

		_isActive = active;

		//enable/disable all relevant components
		foreach (MonoBehaviour script in ScriptComponents)
		{
			if (script != null)
			{
				script.enabled = active;
			}
		}

		if (Rend != null)
		{
			Rend.enabled = active;
		}

		if (Anim != null)
		{
			Anim.enabled = active;
		}

		if (Col != null)
		{
			Col.enabled = active;
		}

		if (Rb != null)
		{
			Rb.bodyType = active ? RigidbodyType2D.Dynamic : Rb.bodyType;
		}
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

	public virtual void PhysicsReEnabled()
	{

	}
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle,
	Nebula
}