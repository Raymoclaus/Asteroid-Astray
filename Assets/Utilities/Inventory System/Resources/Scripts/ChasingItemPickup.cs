using UnityEngine;
using MovementBehaviours;
using GenericExtensions;

namespace InventorySystem
{
	using ParticleSystemControllers;

	[RequireComponent(typeof(ItemPickup))]
	[RequireComponent(typeof(FollowBehaviour))]
	public class ChasingItemPickup : MonoBehaviour
	{
		private FollowBehaviour follow;
		private FollowBehaviour Follow => follow != null ? follow
			: (follow = GetComponent<FollowBehaviour>());
		private ItemPickup pickup;
		[SerializeField] private PickupTrailController pickupTrailPrefab;
		[SerializeField] private SpriteRenderer pickupGlow;
		private PickupTrailController pickupTrail;
		public ItemPickup Pickup => pickup != null ? pickup
			: (pickup = GetComponent<ItemPickup>());
		[SerializeField]
		private float minStartSpeed = 2f,
			maxStartSpeed = 6f;
		[SerializeField] private float idleWaitDuration = 1f;
		private string idleWaitTimerID;
		private bool following = false;

		private IInventoryHolder targetInventoryHolder;

		private void Awake()
		{
			Follow.PhysicsController.CanMove = false;
			float angle = Random.value * 360f;
			float startSpeed = Random.Range(minStartSpeed, maxStartSpeed);
			Follow.PhysicsController.SetVelocity(
				angle.DegreeAngleToVector2() * startSpeed);
			idleWaitTimerID = "Idle Wait Timer" + gameObject.GetInstanceID();
			TimerTracker.AddTimer(idleWaitTimerID, 0f, StartFollowing, null);
			pickupTrail = Instantiate(pickupTrailPrefab, transform.position,
				Quaternion.identity, ParticleGenerator.holder);
			pickupTrail.SetTarget(transform);
			Color col = Color.white;
			pickupTrail.SetColor(col);
			pickupGlow.color = col;
		}

		private void Update()
		{
			if (IsWaiting)
			{
				return;
			}

			if (following)
			{
				Follow.TriggerUpdate();
			}
			else
			{
				Vector3 velocity = Follow.PhysicsController.CurrentVelocity;
				if (velocity == Vector3.zero)
				{
					TimerTracker.SetTimer(idleWaitTimerID, idleWaitDuration);
				}
			}
		}

		public void SetLocation(Vector3 location) => transform.position = location;

		public void SetItem(ItemObject item) => Pickup.SetItemType(item);

		private bool IsWaiting => TimerTracker.GetTimer(idleWaitTimerID) > 0f;

		private void StartFollowing()
		{
			following = true;
			Follow.PhysicsController.CanMove = true;
			Follow.OnReachedTarget.AddListener(GiveItem);
		}

		public void SetTarget(IInventoryHolder inventoryHolder)
		{
			targetInventoryHolder = inventoryHolder;
			Follow.SetTarget(inventoryHolder.DefaultInventory.transform);
		}

		private void GiveItem()
		{
			pickupTrail.SetLooping(false);
			Pickup.SendItem(targetInventoryHolder);
			Destroy(gameObject);
		}
	}
}