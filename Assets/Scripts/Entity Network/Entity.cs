using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	private ChunkCoords _coords;
	public Collider2D Col;
	public Rigidbody2D Rb;
	public bool ShouldDisablePhysicsOnDistance = true;
	public bool ShouldDisableObjectOnDistance = true;
	private bool _isActive = true;
	private bool disabled = false;
	private Vector3 vel;
	private float disableTime;

	//drill related
	public bool canDrill;
	private DrillBit drill;
	protected LayerMask layerDrill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer Rend;
	public Animator Anim;

	public static int physicsActive;

	public static int GetActive()
	{
		return physicsActive;
	}

	public virtual void Awake()
	{
		physicsActive++;
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
				physicsActive++;
				disabled = false;
				gameObject.SetActive(true);
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				physicsActive--;
				disabled = true;
				vel = Rb.velocity;
				disableTime = Time.time;
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

	public void SetAllActivity(bool active)
	{
		if (active == _isActive || !ShouldDisablePhysicsOnDistance) return;

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
	public virtual float DrillDamageQuery()
	{
		return 1f;
	}

	public virtual void PhysicsReEnabled()
	{
		transform.position += vel * (Time.time - disableTime) / 60f * Cnsts.TIME_SPEED;
	}
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle
}