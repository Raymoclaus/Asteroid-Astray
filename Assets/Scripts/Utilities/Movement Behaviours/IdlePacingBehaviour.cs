using UnityEngine;

namespace MovementBehaviours
{
	public class IdlePacingBehaviour : MovementBehaviour
	{
		public bool IsPacing { get; private set; }
		public Vector3 PaceTargetLocation { get; private set; }
		public Bounds pacingBounds;
		public float goalRadius = 0.1f;
		public float paceChance = 0.01f;
		public float paceCooldown = 2f;
		private float paceCooldownTimer;

		protected virtual void Update()
		{
			paceCooldownTimer -= Time.deltaTime;

			if (IsPacing)
			{
				if (IsWithinGoalRadius)
				{
					EndPace();
				}
				else
				{
					MoveTowardsPosition(PaceTargetLocation);
				}
			}
			else
			{
				if (ShouldPace())
				{
					StartPace();
				}
			}
		}

		protected void StartPace()
		{
			IsPacing = true;
			PaceTargetLocation = ChoosePaceLocation;
		}

		protected virtual Vector3 ChoosePaceLocation
		{
			get
			{
				Vector3 location = Vector3.zero;
				int freezeCount = 0;
				do
				{
					freezeCount++;
					if (freezeCount > 1000)
					{
						Debug.Log("No valid pace location found");
						break;
					}
					location = new Vector3(
						Random.Range(pacingBounds.min.x, pacingBounds.max.x),
						Random.Range(pacingBounds.min.y, pacingBounds.max.y),
						Random.Range(pacingBounds.min.z, pacingBounds.max.z));
				} while (Vector3.Distance(SelfPosition, location) <= goalRadius);
				return location;
			}
		}

		protected virtual bool ShouldPace()
			=> paceCooldownTimer <= 0f && Random.value <= paceChance;

		protected void EndPace()
		{
			SlowDown();
			IsPacing = false;
			paceCooldownTimer = paceCooldown;
		}

		protected float DistanceToTargetLocation
			=> Vector3.Distance(SelfPosition, PaceTargetLocation);

		protected bool IsWithinGoalRadius => DistanceToTargetLocation <= goalRadius;
	}

}