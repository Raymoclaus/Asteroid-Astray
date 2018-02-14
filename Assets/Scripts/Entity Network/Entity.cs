using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	private ChunkCoords _coords;
	public Collider2D Col;
	public Rigidbody2D Rb;
	public bool ShouldDisableOnDistance = true;

	//drill related
	public bool canDrill;
	private DrillBit drill;
	protected LayerMask layerDrill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer Rend;
	public Animator Anim;

	public virtual void Awake()
	{
		_coords = new ChunkCoords(transform.position);
		EntityNetwork.AddEntity(this, _coords);
		RepositionInNetwork();

		layerDrill = LayerMask.NameToLayer("Drill");
	}

	public virtual void Start()
	{

	}

	public virtual void LateUpdate()
	{
		RepositionInNetwork();
	}

	private void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		if (newCc != _coords)
			EntityNetwork.Reposition(this, newCc);
	}

	public void SetCoordinates(ChunkCoords newCc)
	{
		ChunkCoords check = new ChunkCoords(transform.position);
		if (newCc == check)
			_coords = newCc;
	}

	public void SetAllActivity(bool active)
	{
		if (!ShouldDisableOnDistance) return;

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
			Rb.bodyType = active ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
		}
	}

	public virtual EntityType GetEntityType()
	{
		return EntityType.Entity;
	}

	public virtual void DestroySelf()
	{
		if (EntityNetwork.Chunk(_coords).Contains(this))
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
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle
}