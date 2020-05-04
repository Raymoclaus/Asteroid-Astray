using InventorySystem;
using SaveSystem;

namespace QuestSystem.Requirements
{
	public class DeliveryQReq : QuestRequirement
	{
		private IDeliveryReceiver _deliveryReceiver;
		private string DeliveryReceiverID { get; set; }
		private IDeliverer _deliverer;
		private string DelivererID { get; set; }
		private QuestDelivery QuestDelivery { get; set; }

		public DeliveryQReq(IDeliveryReceiver deliveryReceiver, IDeliverer deliverer,
			IDelivery delivery, string description, IWaypoint waypoint)
			: base(description, waypoint)
		{
			_deliveryReceiver = deliveryReceiver;
			DeliveryReceiverID = deliveryReceiver.UniqueID;
			_deliverer = deliverer;
			DelivererID = deliverer.UniqueID;
			QuestDelivery = new QuestDelivery(delivery);
		}

		public override void Activate()
		{
			base.Activate();
			_deliveryReceiver.OnDeliveryReceived += EvaluateEvent;
		}

		private void EvaluateEvent(IDeliverer deliverer, IDelivery delivery)
		{
			if (Completed || !active) return;

			if (deliverer != _deliverer) return;

			if (!delivery.Matches(QuestDelivery)) return;

			QuestRequirementCompleted();
		}

		private const string SAVE_TAG_NAME = "Delivery Requirement";
		public override void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save waypoint ID
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, WaypointID);
		}
	}
}