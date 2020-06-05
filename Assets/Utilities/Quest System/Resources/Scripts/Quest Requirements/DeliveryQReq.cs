using InventorySystem;
using SaveSystem;
using UnityEngine;

namespace QuestSystem.Requirements
{
	public class DeliveryQReq : QuestRequirement
	{
		private IDeliveryReceiver _deliveryReceiver;
		private string DeliveryReceiverID { get; set; }
		private IDeliverer _deliverer;
		private string DelivererID { get; set; }
		private QuestDelivery QuestDelivery { get; set; }

		protected DeliveryQReq() : base()
		{

		}

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
			if (Completed)
			{
				Debug.Log("Quest requirement already completed.");
				return;
			}

			if (deliverer != _deliverer) return;

			if (!delivery.Matches(QuestDelivery)) return;

			QuestRequirementCompleted();
		}

		private const string DELIVERY_RECEIVER_ID_VAR_NAME = "Delivery Receiver ID",
			DELIVERER_ID_VAR_NAME = "Deliverer ID",
			DELIVERY_DETAILS_VAR_NAME = "Delivery Details";

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

		protected override bool ApplyData(DataModule module)
		{
			if (base.ApplyData(module)) return true;

			switch (module.parameterName)
			{
				default:
					return false;
				case DELIVERY_RECEIVER_ID_VAR_NAME:
					DeliveryReceiverID = module.data;
					if (DeliveryReceiverID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(DeliveryReceiverID);
						if (obj is IDeliveryReceiver dr)
						{
							_deliveryReceiver = dr;
						}
					}
					break;
				case DELIVERER_ID_VAR_NAME:
					DelivererID = module.data;
					if (DelivererID != string.Empty)
					{
						IUnique obj = UniqueIDGenerator.GetObjectByID(DelivererID);
						if (obj is IDeliverer d)
						{
							_deliverer = d;
						}
					}
					break;
				case DELIVERY_DETAILS_VAR_NAME:
					bool foundVal = DeliveryOrderDetailsGenerator.TryParse(
						module.data,
						out int numItems,
						out int numUniqueItems,
						out string[] orderList);
					if (foundVal)
					{
						DeliveryOrderDetails orderDetails = new DeliveryOrderDetails(numItems, numUniqueItems, orderList);
						QuestDelivery = new QuestDelivery(orderDetails);
					}
					else
					{
						Debug.Log("Delivery Details data could not be parsed.");
					}
					break;
			}

			return true;
		}
	}
}