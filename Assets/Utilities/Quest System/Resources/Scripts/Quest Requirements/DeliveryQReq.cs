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

		private const string REQUIREMENT_TYPE = "Delivery Requirement",
			DELIVERY_RECEIVER_ID_VAR_NAME = "Delivery Receiver ID",
			DELIVERER_ID_VAR_NAME = "Deliverer ID",
			DELIVERY_DETAILS_VAR_NAME = "Delivery Details";

		public override string GetRequirementType() => REQUIREMENT_TYPE;

		public override void Save(string filename, SaveTag parentTag)
		{
			base.Save(filename, parentTag);

			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save delivery receiver ID
			DataModule module = new DataModule(DELIVERY_RECEIVER_ID_VAR_NAME, DeliveryReceiverID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save deliverer ID
			module = new DataModule(DELIVERER_ID_VAR_NAME, DelivererID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save delivery details
			module = new DataModule(DELIVERY_DETAILS_VAR_NAME, QuestDelivery.GetOrderDetails());
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}
}