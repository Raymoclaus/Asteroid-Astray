using AttackData;
using System;
using System.Collections.Generic;
using TriggerSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StraightBlast : MonoBehaviour, IProjectile, IAttackActor
{
	[SerializeField]
	private Rigidbody2D rb;
	private List<StraightBlast> pool;
	private Vector2 firingPos;
	private float maxRange = 10f;
	private float speed = 5f;
	[SerializeField]
	private float damage = 50f;
	private Entity parent;
	private GameObject impactEffect;
	private Vector3 rotation;
	public string UniqueID { get; set; }

	public event Action<IActor> OnDisabled;

	private void OnDisable() => OnDisabled?.Invoke(this);

	public Vector3 FacingDirection => rotation;

	public bool CanTriggerPrompts => false;

	public AttackManager GetAttackManager => GetComponent<AttackManager>();

	public Vector3 Position => transform.position;

	public void Shoot(Vector2 startPos, Quaternion startRot, List<StraightBlast> p, Entity shooter)
	{
		firingPos = startPos;
		rotation = startRot.eulerAngles;
		transform.position = startPos;
		transform.rotation = startRot;
		gameObject.SetActive(true);
		rb.velocity = transform.up * speed;
		pool = p;
		parent = shooter;
		GetAttackManager.AddAttackComponent<OwnerComponent>(shooter);
	}

	private void Update()
	{
		if (Vector2.Distance(transform.position, firingPos) > maxRange)
		{
			Dissipate();
		}
	}

	private void Dissipate()
	{
		pool.Add(this);
		gameObject.SetActive(false);
	}

	public void Hit(Entity obj, Vector2 contactPoint)
	{
		//report damage calculation to the object taking the damage
		//obj.TakeDamage(damage, contactPoint, parent, 1f, true);
		//destroy projectile
		//Dissipate();
	}

	public Entity GetShooter() => parent;

	public void EnteredTrigger(ITrigger vTrigger)
	{

	}

	public void ExitedTrigger(ITrigger vTrigger)
	{

	}

	public void Hit(IAttackTrigger trigger)
	{
		Dissipate();
	}
}