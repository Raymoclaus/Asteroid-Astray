using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingBehaviour : TargetBasedBehaviour
{
	[SerializeField] private float chargeSpeed = 12f;
	public bool IsCharging { get; private set; }
	public Vector3 ChargeDirection { get; private set; }
	public bool IsBouncing { get; private set; }
	private Vector3 wallNormal;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];

	protected virtual void Update()
	{
		if (IsCharging)
		{
			MoveInDirection(ChargeDirection);
		}
		else if (IsBouncing)
		{
			if (IsTouchingWall)
			{
				SetVelocity(wallNormal);
			}
			else
			{
				IsBouncing = false;
			}
		}
		else
		{
			if (TargetIsNearby && ShouldCharge)
			{
				StartCharging();
			}
		}
	}

	protected override void OnCollisionEnter2D(Collision2D collision)
	{
		base.OnCollisionEnter2D(collision);

		Collider2D otherCol = collision.collider;
		GameObject otherObj = otherCol.gameObject;
		int otherLayer = otherObj.layer;
		string otherLayerName = LayerMask.LayerToName(otherLayer);
		Vector3 otherObjPosition = otherObj.transform.position;

		if (string.Compare(otherLayerName, "Default") == 0)
		{
			Debug.Log("Charged into object on Default layer", otherObj);
		}
		else if (otherLayer == WallLayer)
		{
			if (IsCharging)
			{
				StopCharging(true);
				int contactCount = collision.GetContacts(contacts);
				if (contactCount > 0)
				{
					wallNormal = contacts[0].normal;
				}
			}
		}
	}

	protected virtual void StartCharging()
	{
		IsCharging = true;
		ChargeDirection = TargetDirection;
	}

	protected void StopCharging(bool hitWall)
	{
		IsCharging = false;
		IsBouncing = hitWall;
		SlowDown();
	}

	protected override float Speed => chargeSpeed;

	protected virtual bool ShouldCharge => !IsCharging && !IsTouchingWall;
}
