using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttleCtrl : MonoBehaviour {
	/* Fields */
	#region
	[Header("Required references")]
	[Tooltip("Requires reference to the CircleCollider2D on the shuttle.")]
	public CircleCollider2D col;
	[Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
	public SpriteRenderer sprRend;

	[Header("Movement related")]
	[Tooltip("Rate of speed accumulation when moving forward.")]
	public float acceleration;
	[Tooltip("Rate of speed decay.")]
	public float deceleration;
	[Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
	public float speedLimit;
	//maximum rotation speed
	public float maxRotSpeed = 120f;
	//the rotation that the shuttle should be at
	private Vector3 rot;
	//the velocity of the shuttle
	private Vector3 vel;
	//force of acceleration via the shuttle
	private Vector3 accel;
	//used to calculate where to move each frame
	private Vector3 calcPos;
	//player can only turn if these conditions are met
	public bool CanTurn { get { return !Dashing && !usingDrill; } }

	//states and properties of the shuttle
	//whether or not the shuttle is in the process of dashing
	private bool Dashing { get { return dashTime > 0f; } }
	//amount of time a dash lasts (depends on what causes the dash)
	private float dashTime;
	//value affecting how fast you move during a dash (depends on what causes the dash)
	private float dashSpeed;
	//whether the shuttle is above speed limit
	//This can be true during a dash and as you begin decelerating back to the speed limit
	private bool Speeding { get { return vel.magnitude > speedLimit; } }

	[Header("Drill Attack related")]
	[Tooltip("Reference to the object at the drill's position.")]
	public Transform drillPos;
	[Tooltip("The angle towards an object the shuttle must be facing to be considered able to drill the object.")]
	public float drillAngle;
	[Tooltip("The amount of time the ship can continue dashing due to a drill attack before speed begins decaying.")]
	public float drillDashTime;
	[Tooltip("The intensity of the speed boost. Equation is Drill Dash Speed * Speed Limit.")]
	public float drillDashSpeed;
	[Tooltip("Amount of damage dealt when using the drill. A value of 1 means 1 damage per second.")]
	public float drillDamage;
	//true when drill dash begins and returns back to false when no longer speeding or drilling
	private bool usingDrill;
	//true when touching a drillable-object while using the drill
	private bool drilling;
	//reference to object being drilled if any
	private DrillableObject drilledObject;
	#endregion

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (!Dashing) {
				usingDrill = true;
				dashTime = drillDashTime;
				dashSpeed = drillDashSpeed;
			}
		}
		if (Input.GetKeyUp(KeyCode.Space)) {
			if (Dashing) {
				dashTime = 0f;
			}
			if (drilling) {
				drilling = false;
				usingDrill = false;
				drilledObject = null;
			}
		}

		if (drilling) {
			ShakeEffect.Shake(sprRend.transform, 0.01f);
			DrillObject();
		} else {
			sprRend.transform.localPosition = Vector2.zero;
		}

		UpdateStateBooleans();
	}

	void FixedUpdate() {
		//get shuttle movement input
		GetMovementInput();
		//calculate position based on input
		CalculatePosition();
		//actually move the object to the calculated position
		ApplyMovement();
	}

	private void GetMovementInput() {
		//get rotation input
		Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float angle = Vector2.Angle(Vector2.up, cursorPos - transform.position);
		if (transform.position.x < cursorPos.x) {
			angle = 180f + (180f - angle);
		}
		rot.z = Mathf.MoveTowardsAngle(rot.z, angle, Time.fixedDeltaTime * maxRotSpeed);

		//get movement input
		if (Input.GetKey(KeyCode.W)) {
			accel.y += acceleration;
		}
		if (Input.GetKey(KeyCode.S)) {
			accel.y -= acceleration;
		}
		if (Input.GetKey(KeyCode.A)) {
			accel.x -= acceleration;
		}
		if (Input.GetKey(KeyCode.D)) {
			accel.x += acceleration;
		}

		//normalise acceleration
		if (accel != Vector3.zero) {
			accel.Normalize();
			accel *= acceleration;
		}
	}

	//use calculated rotation and speed to determine where to move to
	private void CalculatePosition() {
		float decelerationModifier = 1f;
		//apply speed limit
		if (Speeding) {
			decelerationModifier += 1f;
		}
		//apply acceleration
		vel += accel;
		accel = Vector3.zero;
		//apply deceleration
		vel = Vector3.MoveTowards(vel, Vector3.zero, deceleration * decelerationModifier);
		//use dash speed if dashing
		if (Dashing) {
			vel.Normalize();
			vel *= speedLimit * dashSpeed;
			dashTime -= Time.fixedDeltaTime;
		}
		//set rotation
		transform.eulerAngles = rot;
		//move forward at calculated speed
		calcPos += vel;
	}

	//gradually transforms object to calculated position until it is either reached or is interrupted by a collision
	private void ApplyMovement() {
		//make sure that if a loop has been running too long, break from it
		int freezeCheck = 0;
		//determine where the object should be relative to its starting position
		Vector3 newPos = transform.position + calcPos;
		//reusable Vector2 for direction of collision checking
		Vector2 directionCheck = Vector2.zero;

		//iterate through loop until object reaches new position
		while (transform.position != newPos) {
			//the stop check will make sure the loop is broken if the object cannot move further due to collision
			Vector3 stopCheck = transform.position;

			//move vertically if no collision is detected
			directionCheck = Vector2.up * Cnsts.ACCURACY * (newPos.y > transform.position.y ? 1f : -1f);
			if (!CheckDirection(directionCheck)) {
				transform.position = Vector3.MoveTowards(
					transform.position, Vector3.up * newPos.y + Vector3.right * transform.position.x, Cnsts.ACCURACY);
			} else {
				CollisionDetected(directionCheck);
			}
			//move horizontally if no collision is detected
			directionCheck = Vector2.right * Cnsts.ACCURACY * (newPos.x > transform.position.x ? 1f : -1f);
			if (!CheckDirection(directionCheck)) {
				transform.position = Vector3.MoveTowards(
					transform.position, Vector3.right * newPos.x + Vector3.up * transform.position.y, Cnsts.ACCURACY);
			} else {
				CollisionDetected(directionCheck);
			}

			//if position hasn't changed then assume the direction is blocked by collision
			if (stopCheck == transform.position) {
				break;
			}

			//increment freeze check and break out of loop if it's been running too long
			freezeCheck++;
			if (freezeCheck > Cnsts.FREEZE_LIMIT) {
				break;
			}
		}

		//reset calculated position
		calcPos = Vector2.zero;
	}

	private void CollisionDetected(Vector2 direction) {
		//get collider of the object being collided with
		Collider2D other = CheckCollider(direction);

		//if using drill then check if other object is drillable
		if (usingDrill && !drilling) {
			//check if other is a drillable object
			if (drilledObject == null && other.CompareTag("DrillableObject")) {
				if (CheckDrillAngle(other.transform.position)) {
					drilledObject = other.GetComponent<DrillableObject>();
					drilling = true;
				}
			}
		}
	}

	//Compares the angle of the other object with the up vector of the shuttle.
	//Returns whether then angle is within the threshold.
	private bool CheckDrillAngle(Vector2 objPos) {
		float otherAngle = Vector2.Angle(Vector2.up, objPos - (Vector2)drillPos.position + Vector2.up);
		if (objPos.x < drillPos.position.x) {
			otherAngle = 180f + (180f - otherAngle);
		}
		Debug.Log(otherAngle);
		Debug.Log(rot.z);
		Debug.Log(Mathf.DeltaAngle(otherAngle, rot.z));
		return Mathf.Abs(Mathf.DeltaAngle(otherAngle, rot.z)) <= drillAngle / 2f;
	}

	private void DrillObject() {
		if (drilledObject.TakeDrillDamage(Time.deltaTime * drillDamage)) {
			drilling = false;
			usingDrill = false;
			drilledObject = null;
		}
	}

	private void UpdateStateBooleans() {
		if (!Speeding) {
			if (usingDrill) {
				usingDrill = drilling || Dashing;
			}
		}
	}

	//Checks for a collision with "Solid" (such as asteroids) in the given direction
	private bool CheckDirection(Vector2 dir) {
		return Physics2D.OverlapCircle(
			(Vector2)col.bounds.center + dir, col.radius, 1 << LayerMask.NameToLayer("Solid"));
	}
	//Same as above CheckDirection but return the collider of the object being collided with instead of a boolean
	private Collider2D CheckCollider(Vector2 dir) {
		return Physics2D.OverlapCircle(
			(Vector2)col.bounds.center + dir, col.radius, 1 << LayerMask.NameToLayer("Solid"));
	}

	private void SetRot(float newRot) {
		rot.z = Mathf.Abs(newRot % 360f);
	}

	private void AddRot(float addRot) {
		rot.z = Mathf.Abs((rot.z + addRot) % 360f);
	}
}
