using CustomDataTypes;
using InventorySystem;
using QuestSystem;
using QuestSystem.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using GenericExtensions;
using TriggerSystem;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	[SerializeField] private LimitedScriptedDrops scriptedDrops;
	public static Character MainCharacter { get; private set; }
	public static Action OnMainCharacterUpdated;
	private Quester MainQuester { get; set; }
	[SerializeField] private IInteractor playerTriggerer;
	private IInteractor PlayerTriggerer
		=> playerTriggerer ?? (playerTriggerer = MainCharacter.GetComponent<IInteractor>());
	private MainHatchPrompt _mainHatch;
	private MainHatchPrompt MainHatch
		=> _mainHatch ?? (_mainHatch = FindObjectOfType<MainHatchPrompt>());
	private IActionTrigger MainHatchTrigger
		=> MainHatch.GetComponentInChildren<IActionTrigger>();
	[SerializeField] private DerangedSoloBot derangedSoloBotPrefab;
	private TutorialPrompts tutPrompts;
	private TutorialPrompts TutPrompts
		=> tutPrompts ?? (tutPrompts = FindObjectOfType<TutorialPrompts>());

	public bool CanSendDialogue { get; set; } = true;

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

	[SerializeField] private Entity botHivePrefab, soloBotPrefab, mainCharacterPrefab;

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

		EntityGenerator.AddListener(CreateMainCharacter);
	}

	private void OnDestroy()
	{
		if (MainQuester == null) return;
		MainQuester.OnQuestCompleted += EvaluateQuest;
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

	private void CreateMainCharacter()
	{
		if (MainCharacter != null) return;

		SpawnableEntity se = EntityGenerator.GetSpawnableEntity(mainCharacterPrefab);
		if (se == null) return;

		List<Entity> spawnedEntities = EntityGenerator.SpawnEntity(se);
		Character c = (Character)spawnedEntities.FirstOrDefault(t => t.IsA<Character>());
		SetMainCharacter(c);
	}

	private void SetMainCharacter(Character c)
	{
		if (c == MainCharacter) return;

		if (MainQuester != null)
		{
			MainQuester.OnQuestCompleted -= EvaluateQuest;
		}

		MainCharacter = c;
		MainQuester = MainCharacter.GetComponentInChildren<Quester>();
		OnMainCharacterUpdated?.Invoke();

		if (MainQuester != null)
		{
			MainQuester.OnQuestCompleted += EvaluateQuest;
		}
	}

	private void EvaluateQuest(Quest completedQuest)
	{
		if (completedQuest.CompareName(FirstGatheringQuest))
		{
			CompletedFirstGatheringQuest();
		}
		else if (completedQuest.CompareName(CraftYourFirstRepairKitQuest))
		{
			CompletedCraftYourFirstRepairKitQuest();
		}
		else if (completedQuest.CompareName(RepairTheShuttleQuest))
		{
			CompletedRepairTheShuttleQuest();
		}
		else if (completedQuest.CompareName(ReturnToTheShipQuest))
		{
			CompletedReturnToTheShipQuest();
		}
		else if (completedQuest.CompareName(FindEnergySourceQuest))
		{
			CompletedFindEnergySourceQuest();
		}
		else if (completedQuest.CompareName(RechargeTheShipQuest))
		{
			CompletedRechargeTheShipQuest();
		}
	}

	public void StartFirstGatheringQuest()
	{
		if (MainQuester == null) return;

		GiveQuest(MainQuester, FirstGatheringQuest);
		ActivateScriptedDrops(true);
		StartPassiveDialogue(useThrustersConversation);
		TutPrompts?.drillInputPromptInfo.SetIgnore(false);
	}

	private Quest m_firstGatheringQuest;
	private Quest FirstGatheringQuest
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
				MainQuester, qRewards, qReqs);

			return m_firstGatheringQuest;
		}
	}

	private void CompletedFirstGatheringQuest()
	{
		ActivateScriptedDrops(false);
		StartCraftYourFirstRepairKitQuest();
		StartPassiveDialogue(completedFirstGatheringQuestConversation);
		TutPrompts?.drillInputPromptInfo.SetIgnore(true);
	}

	public void StartCraftYourFirstRepairKitQuest()
	{
		if (MainCharacter == null) return;

		GiveQuest(MainQuester, CraftYourFirstRepairKitQuest);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(false);
	}

	private Quest m_craftYourFirstRepairKitQuest;
	private Quest CraftYourFirstRepairKitQuest
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
				MainQuester, qRewards, qReqs);

			return m_craftYourFirstRepairKitQuest;
		}
	}

	private void CompletedCraftYourFirstRepairKitQuest()
	{
		StartRepairTheShuttleQuest();
		StartPassiveDialogue(useRepairKitConversation);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
	}

	public void StartRepairTheShuttleQuest()
	{
		if (MainCharacter == null) return;

		GiveQuest(MainQuester, RepairTheShuttleQuest);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(false);
	}

	private Quest m_repairTheShuttleQuest;
	private Quest RepairTheShuttleQuest
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
				MainQuester, qRewards, qReqs);

			return m_repairTheShuttleQuest;
		}
	}

	private void CompletedRepairTheShuttleQuest()
	{
		StartReturnToTheShipQuest();
		StartPassiveDialogue(findShipConversation);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
	}

	public void StartReturnToTheShipQuest()
	{
		if (MainCharacter == null) return;

		GiveQuest(MainQuester, ReturnToTheShipQuest);
	}

	private Quest m_returnToTheShipQuest;
	private Quest ReturnToTheShipQuest
	{
		get
		{
			if (m_returnToTheShipQuest != null) return m_returnToTheShipQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();
			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainCharacter);
			qReqs.Add(new WaypointQReq(wp, "Return to the ship."));

			m_returnToTheShipQuest = new Quest(
				"Find the ship",
				"Communication and Navigation systems on the shuttle have been restored," +
				" but we still can't contact Dendro. Find your way back to the ship and" +
				" check if Dendro is still alright.",
				MainQuester, qRewards, qReqs);

			return m_returnToTheShipQuest;
		}
	}

	private void CompletedReturnToTheShipQuest()
	{
		StartActiveDialogue(foundShipConversation);
	}

	public void StartFindEnergySourceQuest()
	{
		if (MainCharacter == null) return;

		GiveQuest(MainQuester, FindEnergySourceQuest);
	}

	private Quest m_findEnergySourceQuest;
	private Quest FindEnergySourceQuest
	{
		get
		{
			if (m_findEnergySourceQuest != null) return m_findEnergySourceQuest;

			//TODO: This can potentially be exploited if the game is saved and quit during quest.
			//Restarting the quest will cause another bot to be created.
			//Check if a deranged bot already exists.

			//create a deranged bot
			ChunkCoords emptyChunk = EntityGenerator.GetNearbyEmptyChunk();
			SpawnableEntity se = EntityGenerator.GetSpawnableEntity("deranged bot");
			Entity newEntity = EntityGenerator.SpawnOneEntityInChunk(se, emptyChunk);

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();
			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(newEntity, 1f, MainCharacter);
			Action waypointReachedAction = null;
			waypointReachedAction = () =>
			{
				StartActiveDialogue(foundDerangedBotConversation);
				wp.OnWaypointReached -= waypointReachedAction;
			};
			wp.OnWaypointReached += waypointReachedAction;
			qReqs.Add(new GatheringQReq(Item.GetItemByName("Corrupted Corvorite"),
				MainCharacter, "Find the nearby energy source.", wp));

			m_findEnergySourceQuest = new Quest(
				"Acquire an Energy Source",
				"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
				MainQuester, qRewards, qReqs);

			return m_findEnergySourceQuest;
		}
	}

	private void CompletedFindEnergySourceQuest()
	{
		StartRechargeTheShipQuest();
		StartPassiveDialogue(acquiredEnergySourceConversation);
	}

	public void StartRechargeTheShipQuest()
	{
		if (MainCharacter == null) return;

		GiveQuest(MainQuester, RechargeTheShipQuest);
	}

	private Quest m_rechargeTheShipQuest;
	private Quest RechargeTheShipQuest
	{
		get
		{
			if (m_rechargeTheShipQuest != null) return m_rechargeTheShipQuest;

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();

			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainCharacter);
			ItemCollection delivery = new ItemCollection(new ItemStack(Item.GetItemByName("Corrupted Corvorite")));
			MainHatch.ExpectDelivery(MainCharacter, delivery);
			qReqs.Add(new DeliveryQReq(MainHatch, MainCharacter, delivery, "Deliver the energy source to the ship.", wp));

			m_rechargeTheShipQuest = new Quest(
				"Recharge the Ship",
				"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
				MainQuester, qRewards, qReqs);

			return m_rechargeTheShipQuest;
		}
	}

	private void CompletedRechargeTheShipQuest()
	{
		StartActiveDialogue(_rechargingTheShipDialogue);
	}

	public void BringCharacterThroughMainHatch()
	{
		if (MainCharacter is IHatchEnterer obj)
		{
			MainHatch.BringObjectThroughHatch(obj);
		}
	}

	public void ActivateScriptedDrops(bool activate)
		=> scriptedDrops.scriptedDropsActive = activate;

	public bool TakeItem(ItemObject type, int amount) => MainCharacter.TakeItem(type, amount);

	private void GiveQuest(Quester quester, Quest q) => quester.AcceptQuest(q);

	private ConversationWithActions GetConversation(ConversationEvent ce)
	{
		return conversations.FirstOrDefault(t => t.conversationEvent == ce);
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
}
