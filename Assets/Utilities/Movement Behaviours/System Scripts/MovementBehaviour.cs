using UnityEngine;

namespace MovementBehaviours
{
	[RequireComponent(typeof(IPhysicsController))]
	public abstract class MovementBehaviour : MonoBehaviour
	{
		[SerializeField] private bool triggerSelfUpdate = true;

		public delegate void RollEventHandler(Vector3 direction);
		public event RollEventHandler OnRoll;
		protected void TriggerRoll(Vector3 direction) => OnRoll?.Invoke(direction);
		public delegate void BlockEventHandler(Vector3 direction);
		public event BlockEventHandler OnBlock;
		protected void TriggerBlock(Vector3 direction) => OnBlock?.Invoke(direction);
		public delegate void StopBlockingEventHandler();
		public event StopBlockingEventHandler OnStopBlocking;
		protected void TriggerStopBlocking() => OnStopBlocking?.Invoke();

		private IPhysicsController physicsController;
		public IPhysicsController PhysicsController
			=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());

		private static int wallLayer = -1;
		protected static int WallLayer => wallLayer == -1 ?
			wallLayer = LayerMask.NameToLayer("Wall")
			: wallLayer;
		private static int attackLayer = -1;
		protected static int AttackLayer => attackLayer == -1 ?
			attackLayer = LayerMask.NameToLayer("Attack")
			: attackLayer;

		[SerializeField] private float originalSpeed = 5f;

		protected void Update()
		{
			if (!triggerSelfUpdate) return;
			TriggerUpdate();
		}

		public virtual void TriggerUpdate() { }

		protected virtual void OnCollisionEnter2D(Collision2D collision)
		{
			Collider2D otherCol = collision.collider;
			GameObject otherObj = otherCol.gameObject;
			int otherLayer = otherObj.layer;
			if (otherLayer == WallLayer)
			{
				IsTouchingWall = true;
			}
		}

		protected virtual void OnCollisionExit2D(Collision2D collision)
		{
			Collider2D otherCol = collision.collider;
			GameObject otherObj = otherCol.gameObject;
			int otherLayer = otherObj.layer;
			if (otherLayer == WallLayer)
			{
				IsTouchingWall = false;
			}
		}

		public bool IsTouchingWall { get; private set; }

		protected Vector3 SelfPosition => PhysicsController?.SelfPosition ?? transform.position;

		public float OriginalSpeed => originalSpeed;

		protected virtual float Speed => OriginalSpeed;

		public Vector3 MovementDirection => PhysicsController?.MovementDirection ?? Vector3.up;

		protected virtual float MovementSmoothingPower => 0f;

		protected void MoveTowardsPosition(Vector3 targetPosition)
			=> PhysicsController?.MoveTowardsPosition(targetPosition, Speed, MovementSmoothingPower);

		protected void MoveInDirection(Vector3 direction)
			=> PhysicsController?.MoveInDirection(direction, Speed);

		protected void MoveAwayFromPosition(Vector3 position)
			=> PhysicsController?.MoveAwayFromPosition(position, Speed);

		public void SlowDown() => PhysicsController?.SlowDown();

		public void Stop() => PhysicsController?.Stop();

		protected void SetVelocity(Vector3 direction)
			=> PhysicsController?.SetVelocity(direction);

		protected void FaceDirection(Vector3 direction)
			=> PhysicsController?.FaceDirection(direction);
	}
}