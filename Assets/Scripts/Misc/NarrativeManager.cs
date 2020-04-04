using CustomDataTypes;
using InventorySystem;
using QuestSystem;
using QuestSystem.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using StatisticsTracker;
using TriggerSystem;
using TriggerSystem.Triggers;
using UnityEngine;

public class NarrativeManager : MonoBehaviour, IChatter
{
	[SerializeField] private BoolStatTracker shipRechargedStat;
	[SerializeField] private LimitedScriptedDrops scriptedDrops;
	[SerializeField] private Character mainChar;
	private Character MainChar => mainChar != null ? mainChar
		: (mainChar = FindObjectOfType<Character>());
	private Quester MainQuester => MainChar?.GetComponent<Quester>();
	[SerializeField] private IInteractor playerTriggerer;
	private IInteractor PlayerTriggerer
		=> playerTriggerer ?? (playerTriggerer = MainChar.GetComponent<IInteractor>());
	private MainHatchPrompt mainHatch;
	private MainHatchPrompt MainHatch
		=> mainHatch ?? (mainHatch = FindObjectOfType<MainHatchPrompt>());
	private IActionTrigger MainHatchTrigger
		=> MainHatch.GetComponentInChildren<IActionTrigger>();
	[SerializeField] private DerangedSoloBot derangedSoloBotPrefab;
	private TutorialPrompts tutPrompts;
	private TutorialPrompts TutPrompts
		=> tutPrompts ?? (tutPrompts = FindObjectOfType<TutorialPrompts>());

	public bool CanSendDialogue { get; set; } = true;

	[SerializeField] private TY4PlayingUI ty4pUI;
	[SerializeField] private ConversationWithActions
		useThrustersConversation,
		completedFirstGatheringQuestConversation,
		useRepairKitConversation,
		findShipConversation,
		foundShipConversation,
		foundDerangedBotConversation,
		acquiredEnergySourceConversation,
		rechargedTheShipConversation,
		questionHowToObtainEnergySourceConversation;

	private List<ConversationWithActions> conversations;

	[Header("Entity Profiles")]
	[SerializeField] private CharacterProfile claire;

	[SerializeField] private Entity botHivePrefab, soloBotPrefab;

	public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
	public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;

	private void Awake()
	{
		conversations = new List<ConversationWithActions>
		{
			useThrustersConversation,
			completedFirstGatheringQuestConversation,
			useRepairKitConversation,
			findShipConversation,
			foundShipConversation,
			foundDerangedBotConversation,
			acquiredEnergySourceConversation,
			rechargedTheShipConversation,
			questionHowToObtainEnergySourceConversation
		};
	}

	private void OnEnable()
	{
		Quester quester = MainQuester;
		if (quester == null) return;
		quester.OnQuestCompleted += EvaluateQuest;
	}

	private void OnDisable()
	{
		Quester quester = MainQuester;
		if (quester == null) return;
		quester.OnQuestCompleted -= EvaluateQuest;
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
		StartDialogue(useThrustersConversation, true);
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
				MainChar, "Obtain {0} {1} from asteroids: {2} / {0}"));

			qReqs.Add(new GatheringQReq(Item.GetItemByName("Iron"),
				MainChar, "Obtain {0} {1} from asteroids: {2} / {0}"));

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
		StartDialogue(completedFirstGatheringQuestConversation, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(true);
	}

	public void StartCraftYourFirstRepairKitQuest()
	{
		if (MainChar == null) return;

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
				MainChar, "Construct {0} {1} using 2 copper and 1 iron"));

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
		StartDialogue(useRepairKitConversation, true);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
	}

	public void StartRepairTheShuttleQuest()
	{
		if (MainChar == null) return;

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
			qReqs.Add(new ItemUseQReq(Item.GetItemByName("Repair Kit"), MainChar));

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
		StartDialogue(findShipConversation, true);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
	}

	public void StartReturnToTheShipQuest()
	{
		if (MainChar == null) return;

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
			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainChar);
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
		StartDialogue(foundShipConversation, false);
	}

	public void StartFindEnergySourceQuest()
	{
		if (MainChar == null) return;

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
			Entity newEntity = EntityGenerator.SpawnOneEntityInChunk(se, null, emptyChunk);

			List<QuestReward> qRewards = new List<QuestReward>();

			List<QuestRequirement> qReqs = new List<QuestRequirement>();
			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(newEntity, 1f, MainChar);
			Action waypointReachedAction = null;
			waypointReachedAction = () =>
			{
				StartDialogue(foundDerangedBotConversation, false);
				wp.OnWaypointReached -= waypointReachedAction;
			};
			wp.OnWaypointReached += waypointReachedAction;
			qReqs.Add(new GatheringQReq(Item.GetItemByName("Corrupted Corvorite"),
				MainChar, "Find the nearby energy source.", wp));

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
		StartDialogue(acquiredEnergySourceConversation, true);
	}

	public void StartRechargeTheShipQuest()
	{
		if (MainChar == null) return;

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

			AttachableWaypoint wp = WaypointManager.CreateAttachableWaypoint(MainHatch, 1f, MainChar);
			qReqs.Add(new InteractionQReq(MainHatch, MainChar, "Return to the ship.", wp));

			m_rechargeTheShipQuest = new Quest(
				"Recharge the Ship",
				"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
				MainQuester, qRewards, qReqs);

			return m_rechargeTheShipQuest;
		}
	}

	private void CompletedRechargeTheShipQuest()
	{
		StartDialogue(rechargedTheShipConversation, false);
		TakeItem(Item.GetItemByName("Corrupted Corvorite"), 1);
		MainHatch.IsLocked = false;
		shipRechargedStat.SetValue(true);
	}

	public void ActivateScriptedDrops(bool activate)
		=> scriptedDrops.scriptedDropsActive = activate;

	public void TakeItem(ItemObject type, int amount) => MainChar.TakeItem(type, amount);

	private void GiveQuest(Quester quester, Quest q) => quester.AcceptQuest(q);

	private ConversationWithActions GetConversation(ConversationEvent ce)
	{
		return conversations.FirstOrDefault(t => t.conversationEvent == ce);
	}

	public void StartActiveDialogue(ConversationEvent ce)
	{
		StartDialogue(GetConversation(ce), false);
	}

	public void StartPassiveDialogue(ConversationEvent ce)
	{
		StartDialogue(GetConversation(ce), true);
	}

	public void StartDialogue(ConversationWithActions ce, bool chat)
	{
		if (chat)
		{
			OnSendPassiveDialogue?.Invoke(ce, true);
			return;
		}
		OnSendActiveDialogue?.Invoke(ce, true);
	}

	public void AllowSendingDialogue(bool allow)
	{
		CanSendDialogue = allow;
	}
}
