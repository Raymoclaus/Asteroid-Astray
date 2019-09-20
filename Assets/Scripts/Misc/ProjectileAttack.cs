using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileAttack : MonoBehaviour
{
	private Rigidbody2D rb;
	public Rigidbody2D Rb => rb ?? (rb = GetComponent<Rigidbody2D>());

	public void SetVelocity(Vector3 velocity) => Rb.velocity = velocity;
}
