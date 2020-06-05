using InputHandlerSystem;
using InventorySystem;
using QuestSystem;
using SaveSystem;
using StatisticsTracker;
using System;
using System.Collections.Generic;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver, IWaypointable, IDeliveryReceiver
{
	[SerializeField] private Animator anim;
	[SerializeField] private BoolStatTracker _shipIsPoweredUpStat;

	private NarrativeManager _narrativeManager;
	private Dictionary<IDeliverer, List<IDelivery>> expectedDeliveries = new Dictionary<IDeliverer, List<IDelivery>>();

	public event Action<IInteractor> OnInteracted;
	public event Action<IDeliverer, IDelivery> OnDeliveryReceived;
	public InvocableOneShotEvent OnLoaded = new InvocableOneShotEvent();

	public string UniqueID { get; set; }

	private void Awake()
	{
		UniqueIDGenerator.OnLoaded.RunWhenReady(Load);
	}

	public Vector3 Position
	{
		get => transform.position;
		set => Debug.Log("The position of this object cannot be changed in this way.", gameObject);
	}

	public void Interacted(IInteractor interactor, GameAction action)
	{
		if (!IsPoweredDown)
		{
			if (interactor is IHatchEnterer hatchEnterer)
			{
				BringObjectThroughHatch(hatchEnterer);
			}
		}

		OnInteracted?.Invoke(interactor);

		interactor.Interact(this);
	}

	public void BringObjectThroughHatch(IHatchEnterer obj)
	{
		Open();
		obj.EnterHatch(Position);
	}

	public bool IsPoweredDown => !_shipIsPoweredUpStat.Value;

	public void SetPower(bool active) => _shipIsPoweredUpStat.SetValue(active);

	private NarrativeManager NarrativeManager => _narrativeManager != null
		? _narrativeManager
		: (_narrativeManager = FindObjectOfType<NarrativeManager>());

	private void Open() => anim.SetTrigger("Open");

	public bool IsExpectingDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer)) return false;

		foreach (IDelivery d in expectedDeliveries[deliverer])
		{
			if (d.Matches(delivery)) return true;
		}
		return false;
	}

	public void ExpectDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer))
		{
			expectedDeliveries.Add(deliverer, new List<IDelivery>());
		}
		
		expectedDeliveries[deliverer].Add(delivery);
	}

	public bool ReceiveDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!IsExpectingDelivery(deliverer, delivery)) return false;
		bool deliveryReceived = RemoveDelivery(deliverer, delivery);
		if (!deliveryReceived) return false;
		OnDeliveryReceived?.Invoke(deliverer, delivery);
		return true;
	}

	private bool RemoveDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer)) return false;
		IDelivery toRemove = null;
		foreach (IDelivery d in expectedDeliveries[deliverer])
		{
			if (!d.Matches(delivery)) continue;
			toRemove = d;
			break;
		}
		if (toRemove == null) return false;
		return expectedDeliveries[deliverer].Remove(toRemove);
	}

	private string DelivererSaveTagName(IDeliverer deliverer) => $"{DELIVERER_SAVE_TAG_NAME}:{deliverer.UniqueID}";

	private string DeliverySaveTagName(int index) => $"{DELIVERY_SAVE_TAG_NAME}:{index}";

	private const string TEMP_SAVE_FILE_NAME ="MainHatch_tmp",
		PERMANENT_SAVE_FILE_NAME = "MainHatch",
		SAVE_TAG_NAME = "MainHatch",
		EXPECTED_DELIVERIES_SAVE_TAG_NAME = "Expected Deliveries",
		DELIVERER_SAVE_TAG_NAME = "Deliverer",
		DELIVERY_SAVE_TAG_NAME = "Delivery",
		UNIQUE_ID_VAR_NAME = "Unique ID",
		DELIVERY_ID_VAR_NAME = "Delivery ID",
		ORDER_DETAILS_VAR_NAME = "Order Details";

	public void TemporarySave()
	{
		//delete existing temporary file
		DeleteTemporarySave();
		//reopen the file (which will recreate the file if second argument is true)
		UnifiedSaveLoad.OpenFile(TEMP_SAVE_FILE_NAME, true);
		//create main tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
		//save unique ID
		DataModule module = new DataModule(UNIQUE_ID_VAR_NAME, UniqueID);
		UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, mainTag, module);
		//create expected deliveries tag
		SaveTag expectedDeliveriesTag = new SaveTag(EXPECTED_DELIVERIES_SAVE_TAG_NAME, mainTag);
		//loop over expected deliveries
		foreach (IDeliverer deliverer in expectedDeliveries.Keys)
		{
			//create deliverer save tag
			SaveTag delivererSaveTag = new SaveTag(DelivererSaveTagName(deliverer), expectedDeliveriesTag);
			//save deliverer ID
			module = new DataModule(DELIVERY_ID_VAR_NAME, deliverer.UniqueID);
			UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, delivererSaveTag, module);
			//get list of expected deliveries
			List<IDelivery> deliveries = expectedDeliveries[deliverer];
			//loop over deliveries
			for (int i = 0; i < deliveries.Count; i++)
			{
				IDelivery delivery = deliveries[i];
				//create delivery tag
				SaveTag deliverySaveTag = new SaveTag(DeliverySaveTagName(i), delivererSaveTag);
				//save delivery ID
				module = new DataModule(DELIVERY_ID_VAR_NAME, i);
				UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, deliverySaveTag, module);
				//save order details
				module = new DataModule(ORDER_DETAILS_VAR_NAME, delivery.GetOrderDetails());
				UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, deliverySaveTag, module);
			}
		}

		UnifiedSaveLoad.SaveOpenedFile(TEMP_SAVE_FILE_NAME);
	}

	public static void PermanentSave()
	{
		//check if a temporary save file exists
		if (!SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME))
		{
			Debug.LogWarning("No temporary save file exists for the main hatch");
			return;
		}
		//copy data from temporary file
		string text = SaveLoad.LoadText(TEMP_SAVE_FILE_NAME);
		//overwrite the permanent file with the copied data
		SaveLoad.SaveText(PERMANENT_SAVE_FILE_NAME, text);
		//delete the temporary file
		DeleteTemporarySave();
	}

	public static void DeleteTemporarySave()
	{
		//delete existing temporary file
		SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
		//close the file because it was deleted
		UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
	}

	private void Load()
	{
		Debug.Log("Main Hatch Data: Begin Loading");
		//check if save file exists
		bool tempSaveExists = SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME);
		bool permanentSaveExists = SaveLoad.RelativeSaveFileExists(PERMANENT_SAVE_FILE_NAME);
		string filename = null;
		if (tempSaveExists)
		{
			filename = TEMP_SAVE_FILE_NAME;
		}
		else if (permanentSaveExists)
		{
			filename = PERMANENT_SAVE_FILE_NAME;
		}
		else
		{
			Debug.Log("Main Hatch Data: Nothing to load, continuing");
			UniqueIDGenerator.AddObject(this);
			OnLoaded.Invoke();
			return;
		}

		//open the save file
		UnifiedSaveLoad.OpenFile(filename, false);
		//create main tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
		//load unique ID
		DataModule uniqueIDModule = UnifiedSaveLoad.GetModuleOfParameter(filename, mainTag, UNIQUE_ID_VAR_NAME);
		if (uniqueIDModule != DataModule.INVALID_DATA_MODULE)
		{
			UniqueID = uniqueIDModule.data;
		}

		UniqueIDGenerator.AddObject(this);
		OnLoaded.Invoke();
		Debug.Log("Main Hatch Data: Loaded");
	}
}