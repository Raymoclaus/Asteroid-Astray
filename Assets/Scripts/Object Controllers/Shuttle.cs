using UnityEngine;

public class Shuttle : Entity
{
    #region Fields
    [Header("Required references")]
    [Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
    public SpriteRenderer SprRend;
    [Header("Movement related")]
    [Tooltip("Rate of speed accumulation when moving forward.")]
    public float Acceleration = 5f;
    [Tooltip("Rate of speed decay.")]
    public float Deceleration = 1f;
    [Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
    public float SpeedLimit = 10f;
	[Tooltip("When drilling, this is multiplied with the speed limit to allow for faster boost after drilling completes.")]
	public float DrillBoost = 2f;
    //maximum rotation speed
    public float MaxRotSpeed = 10f;
	//used as a temporary storage for rigidbody velocity when the constraints are frozen
	private Vector3 _vel;
    //the rotation that the shuttle should be at
    private Vector3 _rot;
    //force of acceleration via the shuttle
    private Vector2 _accel;
	//store last look direction, useful for joysticks
	private float _lastLookDirection;
    //whether the shuttle is above speed limit
    //This can be true during a dash and as you begin decelerating back to the speed limit
	private bool IsSpeeding
	{
		get
		{
			Vector2 vel = Rb.velocity;
			float sqrMag = vel.sqrMagnitude;
			float spdLimit = SpeedLimit * Cnsts.TIME_SPEED;
			//if going under a quarter the speed limit then ignore later calculations
			if (sqrMag < spdLimit / 4f) return false;
			//if going over the overall speed limit then definitely speeding
			if (sqrMag > spdLimit) return true;
			    
			//formula for ellipsoid, determines if velocity is within range
			//for reference: https://www.maa.org/external_archive/joma/Volume8/Kalman/General.html
			//slightly modified for use with square magnitude for better efficiency
			//half and full would normally be squared
			float rotAngle = Mathf.Deg2Rad * _rot.z;
			float a = vel.x * Mathf.Cos(rotAngle) + vel.y * Mathf.Sin(rotAngle);
			float b = vel.x * Mathf.Sin(rotAngle) - vel.y * Mathf.Cos(rotAngle);
			//speed limit is halved for sideways movement
			float half = spdLimit / 2f;
			float full = spdLimit;
			a *= a;
			b *= b;
			float speedCheck = (a / half) + (b / full);
			return speedCheck > 1;
		}
	}
    #endregion

    private void Update()
    {
        //get shuttle movement input
        GetMovementInput();
		//calculate position based on input
		CalculateForces();
	}

	//Checks for input related to movement and calculates acceleration
	private void GetMovementInput()
    {
		//update rotation variable with transform's current rotation
		_rot.z = transform.eulerAngles.z;

        //get rotation input
        float cursorAngle = InputHandler.GetLookDirection(transform.position);
	        
	    //if no rotation input has been given then use the same as last frame
	    if (float.IsPositiveInfinity(cursorAngle)) cursorAngle = _lastLookDirection;
	        
	    //update last look direction (mostly for joystick use)
	    _lastLookDirection = cursorAngle;

	    //determine how quickly to rotate
	    //rotMod controls how smoothly the rotation happens
	    float rotMod = Mathf.Abs((360f - _rot.z) - cursorAngle);
	    if (rotMod > 180f)
	    {
		    rotMod = Mathf.Abs(rotMod - 360f);
	    }
	    rotMod /= 180f;
	    rotMod = Mathf.Pow(rotMod, 0.8f);
        SetRot(Mathf.MoveTowardsAngle(_rot.z, -cursorAngle, MaxRotSpeed * rotMod * Cnsts.TIME_SPEED));

        //get movement input
	    _accel.y += InputHandler.GetInput("MoveVertical") * Acceleration;
	    _accel.x += InputHandler.GetInput("MoveHorizontal") * Acceleration;
	    float magnitude = _accel.magnitude;

        //if no acceleration then ignore the rest
	    if (Mathf.Approximately(_accel.x, 0f) && Mathf.Approximately(_accel.y, 0f)) return;
	        
	    //if using a joystick then don't affect direction because it doesn't feel intuitive
	    if (InputHandler.GetMode() == InputHandler.InputMode.Keyboard)
	    {
		    //rotate forward acceleration direction to be based on the direction the shuttle is facing
		    float accelAngle = Vector2.Angle(Vector2.up, _accel);
		    if (_accel.x < 0)
		    {
			    accelAngle = 180f + (180f - accelAngle);
		    }
		    Vector2 shuttleDir;
		    shuttleDir.x = Mathf.Sin(Mathf.Deg2Rad * (360f - _rot.z + accelAngle));
		    shuttleDir.y = Mathf.Cos(Mathf.Deg2Rad * (360f - _rot.z + accelAngle));
		    _accel = shuttleDir;
	    }
	        
	    if (magnitude > Acceleration)
	    {
		    magnitude = Acceleration;
	    }
	    _accel *= magnitude;
    }

    //use calculated rotation and speed to determine where to move to
    private void CalculateForces()
    {
        float decelerationModifier = 1f;
        //apply speed limit
        if (IsSpeeding)
        {
            decelerationModifier = Acceleration / 2f;
        }

		Vector3 addForce = _accel * Cnsts.TIME_SPEED;
		//reset acceleration
		_accel = Vector2.zero;

		if (IsDrilling)
		{
			//save rigibody's velocity if it has just started drilling
			if (Rb.constraints != RigidbodyConstraints2D.FreezeAll)
			{
				_vel = Rb.velocity;
			}
			//freeze constraints
			Rb.constraints = RigidbodyConstraints2D.FreezeAll;
			//add potential velocity
			_vel += addForce / 10f;
			//apply a continuous slowdown effect
			_vel = Vector3.MoveTowards(_vel, Vector3.zero, 0.1f);
			//calculate how powerful the drill can be
			float drillSpeedModifier = SpeedLimit * DrillBoost * DrillBoost;
			//set an upper limit so that the drill speed doesn't go too extreme
			if (_vel.sqrMagnitude > drillSpeedModifier)
			{
				_vel.Normalize();
				_vel *= Mathf.Sqrt(drillSpeedModifier);
			}
		}
		else
		{
			//if just recently finished drilling then this will allow the shuttle to get its velocity back instantly
			if (Rb.constraints == RigidbodyConstraints2D.FreezeAll)
			{
				Rb.velocity = _vel;
			}
			//otherwise just add force as normal
			else
			{
				//apply acceleration
				Rb.AddForce(addForce);
			}
			//keep constraints unfrozen
			Rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			Rb.drag = Mathf.MoveTowards(
				Rb.drag,
				Deceleration * decelerationModifier,
				Cnsts.TIME_SPEED / 10f);
			//set rotation
			transform.eulerAngles = _rot;
		}
    }

    private void SetRot(float newRot)
    {
        _rot.z = ((newRot % 360f) + 360f) % 360f;
    }
	
	public override EntityType GetEntityType() {
		return EntityType.Shuttle;
	}

	public override float DrillDamageQuery()
	{
		return InputHandler.IsHoldingBack() ? 0f :_vel.magnitude;
	}
}