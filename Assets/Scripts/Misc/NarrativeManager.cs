using CustomDataTypes;
using DialogueSystem;
using GenericExtensions;
using InventorySystem;
using QuestSystem;
using QuestSystem.Requirements;
using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using TriggerSystem;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	public static Character MainCharacter { get; private set; }
	public static event Action OnMainCharacterUpdated;
	public InvocableOneShotEvent OnLoaded = new InvocableOneShotEvent();
	public bool CanSendDialogue { get; set; } = true;
	[SerializeField] private LimitedScriptedDrops scriptedDrops;
	[SerializeField] private IInteractor playerTriggerer;
	[SerializeField] private Character mainCharacterPrefab;
	[SerializeField] private DerangedSoloBot derangedSoloBotPrefab;
	private MainHatchPrompt _mainHatch;
	private TutorialPrompts tutPrompts;
	private Quester MainQuester { get; set; }

	[SerializeField] private TY4PlayingUI ty4pUI;
	[SerializeField] public ConversationWithActions
		wormholeRecoveryConversation,
		useThrustersConversation,
		completedFirstGatheringQuestConversation,
		useRepairKitConversation,
		findShipConversation,
		foundShipConversation,
		foundDerangedBotConversation,
		acquiredEnergySourceConversation,
		_shuttleNeedsRepairsDialogue,
		_preparingToRechargeShipDialogue,
		_doesntHaveEnergySourceYetDialogue,
		_decidedNotToRechargeTheShipYetDialogue,
		_rechargingTheShipDialogue,
		questionHowToObtainEnergySourceConversation;

	private List<ConversationWithActions> conversations;
	private EntityGenerator _entityGenerator;

	private void Awake()
	{
		conversations = new List<ConversationWithActions>
		{
			wormholeRecoveryConversation,
			useThrustersConversation,
			completedFirstGatheringQuestConversation,
			useRepairKitConversation,
			findShipConversation,
			foundShipConversation,
			foundDerangedBotConversation,
			acquiredEnergySourceConversation,
			_shuttleNeedsRepairsDialogue,
			_preparingToRechargeShipDialogue,
			_doesntHaveEnergySourceYetDialogue,
			_decidedNotToRechargeTheShipYetDialogue,
			questionHowToObtainEnergySourceConversation
		};
	}

	private void Start()
	{
		OneShotEventGroupWait wait = new OneShotEventGroupWait(false,
			UniqueIDGenerator.OnLoaded);

		_entityGenerator = FindObjectOfType<EntityGenerator>();
		if (_entityGenerator != null)
		{
			wait.AddEventToWaitFor(_entityGenerator.OnPrefabsLoaded);
		}

		wait.RunWhenReady(Load);
		wait.Start();
	}

	private void OnDestroy()
	{
		if (MainQuester == null) return;
		MainQuester.OnQuestCompleted += EvaluateCompletedQuest;
		MainQuester.OnQuestAccepted += EvaluateAcceptedQuest;
	}

	public static void AddListener(Action action)
	{
		if (MainCharacter == null)
		{
			OnMainCharacterUpdated += action;
		}
		else
		{
			action?.Invoke();
		}
	}

	private MainHatchPrompt MainHatch
		=> _mainHatch ?? (_mainHatch = FindObjectOfType<MainHatchPrompt>());

	private IActionTrigger MainHatchTrigger
		=> MainHatch.GetComponentInChildren<IActionTrigger>();

	private IInteractor PlayerTriggerer
		=> playerTriggerer ?? (playerTriggerer = MainCharacter.GetComponent<IInteractor>());

	private TutorialPrompts TutPrompts
		=> tutPrompts ?? (tutPrompts = FindObjectOfType<TutorialPrompts>());

	private void CreateMainCharacter()
	{
		Debug.Log("Creating Main Character");

		SpawnableEntity se = EntityGenerator.GetSpawnableEntityByFileName(mainCharacterPrefab.name);
		if (se == null) return;

		List<Entity> spawnedEntities = _entityGenerator.SpawnEntity(se);
		Character c = (Character)spawnedEntities.FirstOrDefault(t => t.IsA<Character>());
		SetMainCharacter(c);
	}

	private void SetMainCharacter(Character c)
	{
		if (c == MainCharacter) return;

		if (MainQuester != null)
		{
			MainQuester.OnQuestCompleted -= EvaluateCompletedQuest;
			MainQuester.OnQuestAccepted -= EvaluateAcceptedQuest;
		}

		bool mainCharacterAlreadyFound = MainCharacter != null;
		MainCharacter = c;
		MainQuester = MainCharacter.GetComponentInChildren<Quester>();
		OnMainCharacterUpdated?.Invoke();

		if (!mainCharacterAlreadyFound)
		{
			OnLoaded.Invoke();
			Debug.Log("Main Character found");
		}

		if (MainQuester != null)
		{
			MainQuester.OnQuestCompleted += EvaluateCompletedQuest;
			MainQuester.OnQuestAccepted += EvaluateAcceptedQuest;
		}
	}

	private void EvaluateAcceptedQuest(Quest acceptedQuest)
	{
		if (acceptedQuest.CompareName(FirstGatheringQuest))
		{
			scriptedDrops.ActivateScriptedDrops(MainCharacter.UniqueID);
			StartPassiveDialogue(useThrustersConversation);
			TutPrompts?.drillInputPromptInfo.SetIgnore(false);
		}
		else if (acceptedQuest.CompareName(CraftYourFirstRepairKitQuest))
		{
			TutPrompts?.pauseInputPromptInfo.SetIgnore(false);
		}
		else if (acceptedQuest.CompareName(RepairTheShuttleQuest))
		{
			TutPrompts?.repairKitInputPromptInfo.SetIgnore(false);
		}
		else if (acceptedQuest.CompareName(ReturnToTheShipQuest))
		{

		}
		else if (acceptedQuest.CompareName(FindEnergySourceQuest))
		{

		}
		else if (acceptedQuest.CompareName(RechargeTheShipQuest))
		{

		}
	}

	private void EvaluateCompletedQuest(Quest completedQuest)
	{
		if (completedQuest.CompareName(FirstGatheringQuest))
		{
			scriptedDrops.DeactivateScriptedDrops();
			StartCraftYourFirstRepairKitQuest();
			StartPassiveDialogue(completedFirstGatheringQuestConversation);
			TutPrompts?.drillInputPromptInfo.SetIgnore(true);
		}
		else if (completedQuest.CompareName(CraftYourFirstRepairKitQuest))
		{
			StartRepairTheShuttleQuest();
			StartPassiveDialogue(useRepairKitConversation);
			TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
		}
		else if (completedQuest.CompareName(RepairTheShuttleQuest))
		{
			StartReturnToTheShipQuest();
			StartPassiveDialogue(findShipConversation);
			TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
		}
		else if (completedQuest.CompareName(ReturnToTheShipQuest))
		{
			StartActiveDialogue(foundShipConversation);
		}
		else if (completedQuest.CompareName(FindEnergySourceQuest))
		{
			StartRechargeTheShipQuest();
			StartPassiveDialogue(acquiredEnergySourceConversation);
		}
		else if (completedQuest.CompareName(RechargeTheShipQuest))
		{
			StartActiveDialogue(_rechargingTheShipDialogue);
		}
	}

	#region Quest Definitions

	private Quest m_firstGatheringQuest;
	public Quest FirstGatheringQuest
	{
		get
		{
			if (m_firstGatheringQuest != null) return m_firstGatheringQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();

			qReqs.Add(new GatheringQReq(Item.GetItemByName("Copper"), 2,
				MainCharacter, "Obtain {0} {1} from asteroids: {2} / {0}"));

			qReqs.Add(new GatheringQReq(Item.GetItemByName("Iron"),
				MainCharacter, "Obtain {0} {1} from asteroids: {2} / {0}"));

			m_firstGatheringQuest = new Quest(
				"Gather materials",
				"We need some materials so that we can repair our communications system. Once that" +
				" is done, we should be able to find our way back to Dendro and the ship.",
				qRewards, qReqs);

			return m_firstGatheringQuest;
		}
	}

	private Quest m_craftYourFirstRepairKitQuest;
	public Quest CraftYourFirstRepairKitQuest
	{
		get
		{
			if (m_craftYourFirstRepairKitQuest != null) return m_craftYourFirstRepairKitQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();
			qReqs.Add(new CraftingQReq(Item.GetItemByName("Repair Kit"),
				MainCharacter, "Construct {0} {1} using 2 copper and 1 iron"));

			m_craftYourFirstRepairKitQuest = new Quest(
				"Construct a Repair Kit",
				"Now that we have the necessary materials, we should try constructing a repair kit.",
				qRewards, qReqs);

			return m_craftYourFirstRepairKitQuest;
		}
	}

	private Quest m_repairTheShuttleQuest;
	public Quest RepairTheShuttleQuest
	{
		get
		{
			if (m_repairTheShuttleQuest != null) return m_repairTheShuttleQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();
			qReqs.Add(new ItemUseQReq(Item.GetItemByName("Repair Kit"), MainCharacter));

			m_repairTheShuttleQuest = new Quest(
				"Repair the Shuttle",
				"Using this repair kit should be enough to fix the communications system. Then we can finally get back to the ship.",
				qRewards, qReqs);

			return m_repairTheShuttleQuest;
		}
	}

	private Quest m_returnToTheShipQuest;
	public Quest ReturnToTheShipQuest
	{
		get
		{
			if (m_returnToTheShipQuest != null) return m_returnToTheShipQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();

			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainCharacter);
			WaypointQReq waypointRequirement = new WaypointQReq(wp, "Return to the ship.");
			qReqs.Add(waypointRequirement);

			m_returnToTheShipQuest = new Quest(
				"Find the ship",
				"Communication and Navigation systems on the shuttle have been restored," +
				" but we still can't contact Dendro. Find your way back to the ship and" +
				" check if Dendro is still alright.",
				qRewards, qReqs);

			return m_returnToTheShipQuest;
		}
	}

	private Quest m_findEnergySourceQuest;
	public Quest FindEnergySourceQuest
	{
		get
		{
			if (m_findEnergySourceQuest != null) return m_findEnergySourceQuest;

			//TODO: This can potentially be exploited if the game is saved and quit during quest.
			//Restarting the quest will cause another bot to be created.
			//Check if a deranged bot already exists.

			//create a deranged bot
			ChunkCoords emptyChunk = _entityGenerator.GetNearbyEmptyChunk();
			SpawnableEntity se = EntityGenerator.GetSpawnableEntityByFileName(derangedSoloBotPrefab.name);
			Entity newEntity = _entityGenerator.SpawnOneEntityInChunk(se, emptyChunk);

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();

			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(newEntity, 1f, MainCharacter);
			GatheringQReq gatheringRequirement = new GatheringQReq(Item.GetItemByName("Corrupted Corvorite"),
				MainCharacter, "Find the nearby energy source.", wp);
			qReqs.Add(gatheringRequirement);

			m_findEnergySourceQuest = new Quest(
				"Acquire an Energy Source",
				"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
				qRewards, qReqs);

			return m_findEnergySourceQuest;
		}
	}

	private Quest m_rechargeTheShipQuest;
	public Quest RechargeTheShipQuest
	{
		get
		{
			if (m_rechargeTheShipQuest != null) return m_rechargeTheShipQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();

			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainCharacter);
			ItemCollection delivery = new ItemCollection(new ItemStack(Item.GetItemByName("Corrupted Corvorite")));
			MainHatch.ExpectDelivery(MainCharacter, delivery);
			DeliveryQReq deliveryRequirement = new DeliveryQReq(MainHatch, MainCharacter, delivery,
				"Deliver the energy source to the ship.", wp);
			qReqs.Add(deliveryRequirement);

			m_rechargeTheShipQuest = new Quest(
				"Recharge the Ship",
				"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
				qRewards, qReqs);

			return m_rechargeTheShipQuest;
		}
	}

	#endregion Quest Definitions

	#region StartQuest Methods

	public void StartQuest(Quest q) => MainQuester.AcceptQuest(q);

	public void StartFirstGatheringQuest() => StartQuest(FirstGatheringQuest);

	public void StartCraftYourFirstRepairKitQuest() => StartQuest(CraftYourFirstRepairKitQuest);

	public void StartRepairTheShuttleQuest() => StartQuest(RepairTheShuttleQuest);

	public void StartReturnToTheShipQuest() => StartQuest(ReturnToTheShipQuest);

	public void StartFindEnergySourceQuest() => StartQuest(FindEnergySourceQuest);

	public void StartRechargeTheShipQuest() => StartQuest(RechargeTheShipQuest);

	#endregion StartQuest Methods

	public void BringCharacterThroughMainHatch()
	{
		if (MainCharacter is IHatchEnterer obj)
		{
			MainHatch.BringObjectThroughHatch(obj);
		}
	}

	private ConversationWithActions GetConversation(ConversationEvent ce)
	{
		return conversations.FirstOrDefault(t => t.conversationEvent.name == ce.name);
	}

	public void StartActiveDialogue(ConversationEvent ce)
	{
		StartActiveDialogue(GetConversation(ce));
	}

	public void StartActiveDialogue(ConversationWithActions cwa)
	{
		ActiveDialogueController.StartConversation(cwa);
	}

	public void StartPassiveDialogue(ConversationEvent ce)
	{
		StartPassiveDialogue(GetConversation(ce));
	}

	public void StartPassiveDialogue(ConversationWithActions cwa)
	{
		PassiveDialogueController.StartConversation(cwa);
	}

	private const string TEMP_SAVE_FILE_NAME = "NarrativeManager_tmp",
		PERMANENT_SAVE_FILE_NAME = "NarrativeManager",
		SAVE_TAG_NAME = "NarrativeManager",
		MAIN_CHARACTER_ID_VAR_NAME = "Main Character ID";

	/// <summary>
	/// Grabs all data from a temporary save file and places it into a permanent one.
	/// </summary>
	public static void PermanentSave()
	{
		//check if a temporary save file exists
		if (!SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME))
		{
			Debug.LogWarning("No temporary save file exists for the entity network");
			return;
		}
		//copy data from temporary file
		string text = SaveLoad.LoadText(TEMP_SAVE_FILE_NAME);
		//overwrite the permanent file with the copied data
		SaveLoad.SaveText(PERMANENT_SAVE_FILE_NAME, text);
		//delete the temporary file
		DeleteTemporarySave();
	}

	/// <summary>
	/// Grabs data from all saveable entities and puts it all into a temporary file.
	/// </summary>
	public void TemporarySave()
	{
		//delete existing temporary file
		DeleteTemporarySave();
		//reopen the file (which will recreate the file if second argument is true)
		UnifiedSaveLoad.OpenFile(TEMP_SAVE_FILE_NAME, true);
		//create a main tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
		//save main character ID
		DataModule module = new DataModule(MAIN_CHARACTER_ID_VAR_NAME, MainCharacter?.UniqueID ?? string.Empty);
		UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, mainTag, module);
		//save the data now in temp memory into a file
		UnifiedSaveLoad.SaveOpenedFile(TEMP_SAVE_FILE_NAME);
	}

	public static void DeleteTemporarySave()
	{
		//delete existing temporary file
		SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
		//close the file because it was deleted
		UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
	}

	public void Load()
	{
		Debug.Log("Narrative Manager: Loading");
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
			Debug.Log("Narrative Manager: Nothing to load, continuing");
			CreateMainCharacter();
			OnLoaded.Invoke();
			return;
		}

		//open the save file
		UnifiedSaveLoad.OpenFile(filename, false);
		//create save tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);

		UnifiedSaveLoad.IterateTagContents(
			filename,
			mainTag,
			parameterCallBack: module => ApplyData(module),
			subtagCallBack: subtag => CheckSubtag(TEMP_SAVE_FILE_NAME, subtag));

		OnLoaded.Invoke();
		Debug.Log("Narrative Manager: Loaded");
	}

	public bool ApplyData(DataModule module)
	{
		switch (module.parameterName)
		{
			default:
				return false;
			case MAIN_CHARACTER_ID_VAR_NAME:
			{
				if (module.data == string.Empty)
				{
					CreateMainCharacter();
				}
				else
				{
					//find main character with ID
					IUnique obj = UniqueIDGenerator.GetObjectByID(module.data);
					if (obj is Character character)
					{
						SetMainCharacter(character);
					}
				}

				break;
			}
		}

		return true;
	}

	public bool CheckSubtag(string filename, SaveTag subtag)
	{
		return false;
	}
}