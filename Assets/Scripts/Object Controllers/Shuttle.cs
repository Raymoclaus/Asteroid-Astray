using UnityEngine;
using Utilities.Input;

namespace Object_Controllers
{
    public class Shuttle : Entity
    {
        #region Fields

        [Header("Required references")]
        [Tooltip("Requires reference to the SpriteRenderer of the shuttle.")]
        public SpriteRenderer SprRend;

        [Header("Movement related")]
        [Tooltip("Rate of speed accumulation when moving forward.")]
        public float Acceleration;

        [Tooltip("Rate of speed decay.")]
        public float Deceleration;

        [Tooltip("If speed is higher than this limit then deceleration is increased to compensate.")]
        public float SpeedLimit;

        //maximum rotation speed
        public float MaxRotSpeed = 120f;
	    
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
			    //if going under a quarter the speed limit then ignore later calculations
			    if (sqrMag < SpeedLimit / 4f) return false;
			    //if going over the overall speed limit then definitely speeding
			    if (sqrMag > SpeedLimit) return true;
			    
			    //speed limit is halved for sideways movement
			    //formula for ellipsoid, determines if velocity magnitude is within range
			    //for reference: https://www.maa.org/external_archive/joma/Volume8/Kalman/General.html
			    float rotAngle = Mathf.Deg2Rad * _rot.z;
			    float normalisedSpeed = SpeedLimit * 10f;
			    float a = vel.x * Mathf.Cos(rotAngle) + vel.y * Mathf.Sin(rotAngle);
			    float b = vel.x * Mathf.Sin(rotAngle) - vel.y * Mathf.Cos(rotAngle);
			    float half = normalisedSpeed / 2f;
			    float full = normalisedSpeed * normalisedSpeed;
			    a *= a;
			    b *= b;
			    half *= half;
			    float speedCheck = (a / half) + (b / full);
			    return speedCheck > 1;
		    }
	    }

        #endregion Fields

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
            SetRot(Mathf.MoveTowardsAngle(_rot.z, -cursorAngle, MaxRotSpeed * rotMod * Cnsts.TimeSpeed));

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
	        
            //apply acceleration
            Rb.AddForce(_accel * Cnsts.TimeSpeed);
            //apply deceleration
	        Rb.drag = Mathf.MoveTowards(
		        Rb.drag,
		        Deceleration * decelerationModifier * Cnsts.TimeSpeed,
		        Cnsts.TimeSpeed / 10f);
	        //reset acceleration
	        _accel = Vector2.zero;
            //set rotation
            transform.eulerAngles = _rot;
        }

        private void SetRot(float newRot)
        {
            _rot.z = ((newRot % 360f) + 360f) % 360f;
        }
	
		public override EntityType GetEntityType() {
			return EntityType.Shuttle;
		}
    }
}