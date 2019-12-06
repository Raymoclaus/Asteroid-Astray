using CustomDataTypes;
using InventorySystem;
using QuestSystem;
using QuestSystem.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using TriggerSystem;
using TriggerSystem.Triggers;
using UnityEngine;

public class NarrativeManager : MonoBehaviour, IChatter
{
	[SerializeField] private ItemObject copper, iron, repairKit, corruptedCorvorite;
	public static bool ShuttleRepaired { get; private set; }
	public static bool ShipRecharged { get; private set; }

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
		recoveryConversation,
		useThrustersConversation,
		completedFirstGatheringQuestConversation,
		useRepairKitConversation,
		findShipConversation,
		foundShipConversation,
		foundDerangedBotConversation,
		questionHowToObtainEnergySourceConversation,
		acquiredEnergySourceConversation,
		rechargedTheShipConversation;

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
			recoveryConversation,
			useThrustersConversation,
			completedFirstGatheringQuestConversation,
			useRepairKitConversation,
			findShipConversation,
			foundShipConversation,
			foundDerangedBotConversation,
			questionHowToObtainEnergySourceConversation,
			acquiredEnergySourceConversation,
			rechargedTheShipConversation
		};
	}

	private void Start()
	{
		ChooseStartingLocation();
		MainHatch.IsLocked = true;
		SetShuttleRepaired(false);
		MainChar.CanAttack = false;
		LoadingController.AddListener(StartRecoveryDialogue);
	}

	private void StartRecoveryDialogue()
	{
		StartDialogue(recoveryConversation, false);
	}

	public void StartFirstGatheringQuest()
	{
		if (MainQuester == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		
		qReqs.Add(new GatheringQReq(copper, 2,
			MainChar, "Obtain {0} {1} from asteroids: {2} / {0}"));
		
		qReqs.Add(new GatheringQReq(iron,
			MainChar, "Obtain {0} {1} from asteroids: {2} / {0}"));

		Quest q = new Quest(
			"Gather materials",
			"We need some materials so that we can repair our communications system. Once that" +
			" is done, we should be able to find our way back to Dendro and the ship.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedFirstGatheringQuest;

		GiveQuest(MainQuester, q);
		ActivateScriptedDrops(true);
		StartDialogue(useThrustersConversation, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(false);
	}

	private void CompletedFirstGatheringQuest(Quest quest)
	{
		ActivateScriptedDrops(false);
		StartCraftYourFirstRepairKitQuest();
		StartDialogue(completedFirstGatheringQuestConversation, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(true);
	}

	public void StartCraftYourFirstRepairKitQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new CraftingQReq(repairKit,
			MainChar, "Construct {0} {1} using 2 copper and 1 iron"));

		Quest q = new Quest(
			"Construct a Repair Kit",
			"Now that we have the necessary materials, we should try constructing a repair kit.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedCraftYourFirstRepairKitQuest;

		GiveQuest(MainQuester, q);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(false);
	}

	private void CompletedCraftYourFirstRepairKitQuest(Quest quest)
	{
		StartRepairTheShuttleQuest();
		StartDialogue(useRepairKitConversation, true);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
	}

	public void StartRepairTheShuttleQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new ItemUseQReq(repairKit, MainChar));

		Quest q = new Quest(
			"Repair the Shuttle",
			"Using this repair kit should be enough to fix the communications system. Then we can finally get back to the ship.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedRepairTheShuttleQuest;

		GiveQuest(MainQuester, q);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(false);
	}

	private void CompletedRepairTheShuttleQuest(Quest quest)
	{
		SetShuttleRepaired(true);
		StartReturnToTheShipQuest();
		StartDialogue(findShipConversation, true);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
	}

	public void StartReturnToTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		InteractionWaypoint wp = InteractionWaypoint.CreateWaypoint(MainChar,
			MainHatchTrigger, MainHatchTrigger.PivotPosition);
		qReqs.Add(new InteractionQReq(wp, "Return to the ship."));

		Quest q = new Quest(
			"Find the ship",
			"Communication and Navigation systems on the shuttle have been restored," +
			" but we still can't contact Dendro. Find your way back to the ship and" +
			" check if Dendro is still alright.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedReturnToTheShipQuest;

		GiveQuest(MainQuester, q);
	}

	private void CompletedReturnToTheShipQuest(Quest quest)
	{
		StartDialogue(foundShipConversation, false);
	}

	public void StartFindEnergySourceQuest()
	{
		if (MainChar == null) return;

		//create a deranged bot
		ChunkCoords emptyChunk = EntityGenerator.GetNearbyEmptyChunk();
		SpawnableEntity se = EntityGenerator.GetSpawnableEntity("deranged bot");
		Entity newEntity = EntityGenerator.SpawnOneEntityInChunk(se, null, emptyChunk);
		//set waypoint to new bot
		//attach dialogue prompt when player approaches bot
		VicinityTrigger entityPrompt = newEntity.GetComponentsInChildren<VicinityTrigger>()
			.Where(t => t.gameObject.layer == LayerMask.NameToLayer("VicinityTrigger")).FirstOrDefault();
		Action<IActor> triggerEnterAction = null;
		triggerEnterAction = (IActor actor) =>
		{
			if (!(actor is Shuttle)) return;
			StartDialogue(foundDerangedBotConversation, false);
			entityPrompt.OnEnteredTrigger -= triggerEnterAction;
		};
		entityPrompt.OnEnteredTrigger += triggerEnterAction;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		Waypoint wp = Waypoint.CreateWaypoint(MainChar, entityPrompt,
			entityPrompt.PivotPosition);
		qReqs.Add(new GatheringQReq(corruptedCorvorite,
			MainChar, "Find the nearby energy source.", wp));

		Quest q = new Quest(
			"Acquire an Energy Source",
			"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedFindEnergySourceQuest;

		GiveQuest(MainQuester, q);
	}

	private void CompletedFindEnergySourceQuest(Quest quest)
	{
		StartRechargeTheShipQuest();
		StartDialogue(acquiredEnergySourceConversation, true);
	}

	public void StartRechargeTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();

		Waypoint wp = Waypoint.CreateWaypoint(MainChar, MainHatchTrigger, MainHatchTrigger.PivotPosition);
		qReqs.Add(new WaypointQReq(wp, "Return to the ship."));

		Quest q = new Quest(
			"Recharge the Ship",
			"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedRechargeTheShipQuest;

		GiveQuest(MainQuester, q);
	}

	private void CompletedRechargeTheShipQuest(Quest quest)
	{
		StartDialogue(rechargedTheShipConversation, false);
		TakeItem(corruptedCorvorite, 1);
		MainHatch.IsLocked = false;
		ShipRecharged = true;
	}

	public void ActivateScriptedDrops(bool activate)
		=> scriptedDrops.scriptedDropsActive = activate;

	public void SetShuttleRepaired(bool repaired)
	{
		DistanceUI.Hidden = !repaired;
		ShuttleRepaired = repaired;
	}

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

	private void ChooseStartingLocation()
	{
		Vector2 pos = MainHatch.transform.position;
		float randomAngle = UnityEngine.Random.value * Mathf.PI * 2f;
		Vector2 randomPos = new Vector2(Mathf.Sin(randomAngle),	Mathf.Cos(randomAngle));
		randomPos *= UnityEngine.Random.value * 50f + 100f;
		if (MainChar != null)
		{
			MainChar.Teleport(pos + randomPos);
		}
	}

	public void AllowSendingDialogue(bool allow)
	{
		CanSendDialogue = allow;
	}
}
