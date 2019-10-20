﻿using CustomDataTypes;
using InventorySystem;
using InventorySystem.UI;
using QuestSystem;
using QuestSystem.Requirements;
using System;
using System.Collections.Generic;
using TriggerSystem;
using TriggerSystem.Triggers;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	public static bool ShuttleRepaired { get; private set; }
	public static bool ShipRecharged { get; private set; }
	
	private DialogueController dlgCtrl;
	private DialogueController DlgCtrl
		=> dlgCtrl != null ? dlgCtrl
		: (dlgCtrl = FindObjectOfType<DialogueController>());
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
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
	[SerializeField] private InventoryUIController inventoryUI;
	private InventoryUIController InventoryUI
		=> inventoryUI ?? (inventoryUI = FindObjectOfType<InventoryUIController>());
	[SerializeField] private TY4PlayingUI ty4pUI;

	[SerializeField] private ConversationWithActions
		recoveryDialogue,
		UseThrustersDialogue,
		completedFirstGatheringQuestDialogue,
		useRepairKitDialogue,
		findShipDialogue,
		foundShipDialogue,
		foundDerangedBotDialogue,
		questionHowToObtainEnergySourceDialogue,
		acquiredEnergySourceDialogue,
		rechargedTheShipDialogue;

	[Header("Entity Profiles")]
	[SerializeField] private EntityProfile claire;

	[SerializeField] private Entity botHivePrefab, soloBotPrefab;

	private void DevCheatSpawn(Entity e)
	{
		Vector2 centerPos = MainChar.transform.position;
		float randomAngle = UnityEngine.Random.value * Mathf.PI * 2f;
		Vector2 randomUnit = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
		Vector2 spawnPos = centerPos + randomUnit * 3f;

		Entity newEntity = Instantiate(e);
		newEntity.Teleport(spawnPos);
	}

	private void Start()
	{
		ChooseStartingLocation();
		MainHatch.IsLocked = true;
		LoadingController.AddListener(StartRecoveryDialogue);
	}

	private void StartRecoveryDialogue()
	{
		ActivateSpotlight(true);
		StartDialogue(recoveryDialogue, false);
	}

	public void StartFirstGatheringQuest()
	{
		if (MainQuester == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();

		const Item.Type copper = Item.Type.Copper;
		qReqs.Add(new GatheringQReq(copper, 2,
			MainChar, $"Obtain # {copper} from asteroids"));

		const Item.Type iron = Item.Type.Iron;
		qReqs.Add(new GatheringQReq(iron,
			MainChar, $"Obtain # {iron} from asteroids"));

		Quest q = new Quest(
			"Gather materials",
			"We need some materials so that we can repair our communications system. Once that" +
			" is done, we should be able to find our way back to Dendro and the ship.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedFirstGatheringQuest;

		GiveQuest(MainQuester, q);
		FirstQuestScriptedDrops.scriptedDropsActive = true;
		StartDialogue(UseThrustersDialogue, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(false);
	}

	private void CompletedFirstGatheringQuest(Quest quest)
	{
		ActivateScriptedDrops(false);
		StartCraftYourFirstRepairKitQuest();
		StartDialogue(completedFirstGatheringQuestDialogue, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(true);
	}

	public void StartCraftYourFirstRepairKitQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		const Item.Type repairKit = Item.Type.RepairKit;
		qReqs.Add(new CraftingQReq(repairKit,
			MainChar, $"Construct # {repairKit} using 2 copper and 1 iron"));

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
		StartDialogue(useRepairKitDialogue, true);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
	}

	public void StartRepairTheShuttleQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		Item.Type repairKit = Item.Type.RepairKit;
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
		StartDialogue(findShipDialogue, true);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
	}

	public void StartReturnToTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		InteractionWaypoint wp = InteractionWaypoint.CreateWaypoint(MainChar, MainHatchTrigger);
		qReqs.Add(new InteractionQReq(wp, "Return to the ship."));

		Quest q = new Quest(
			"Find the ship",
			"Communication and Navigation systems on the shuttle have been restored, but we still can't contact Dendro. Find your way back to the ship and check if Dendro is still alright.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedReturnToTheShipQuest;

		GiveQuest(MainQuester, q);
	}

	private void CompletedReturnToTheShipQuest(Quest quest)
	{
		StartDialogue(foundShipDialogue, false);
	}

	public void StartFindEnergySourceQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQReq(Item.Type.CorruptedCorvorite,
			MainChar, "Find the nearby energy source."));

		Quest q = new Quest(
			"Acquire an Energy Source",
			"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
			MainQuester, qRewards, qReqs);
		q.OnQuestComplete += CompletedFindEnergySourceQuest;

		GiveQuest(MainQuester, q);
		//create a deranged bot
		ChunkCoords emptyChunk = EntityGenerator.GetNearbyEmptyChunk();
		SpawnableEntity se = EntityGenerator.GetSpawnableEntity(derangedSoloBotPrefab);
		Entity newEntity = EntityGenerator.SpawnOneEntityInChunk(se, null, emptyChunk);
		//set waypoint to new bot
		//attach dialogue prompt when player approaches bot
		VicinityTrigger entityPrompt = newEntity.GetComponentInChildren<VicinityTrigger>();
		Action<IActor> triggerEnterAction = null;
		triggerEnterAction = (IActor actor) =>
		{
			if (!(actor is Shuttle)) return;
			StartDialogue(foundDerangedBotDialogue, true);
			entityPrompt.OnEnteredTrigger -= triggerEnterAction;
		};
		entityPrompt.OnEnteredTrigger += triggerEnterAction;
	}

	private void CompletedFindEnergySourceQuest(Quest quest)
	{
		StartRechargeTheShipQuest();
		StartDialogue(acquiredEnergySourceDialogue, true);
	}

	public void StartRechargeTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();

		Waypoint wp = Waypoint.CreateWaypoint(MainChar, MainHatchTrigger);
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
		StartDialogue(rechargedTheShipDialogue, false);
		TakeItem(Item.Type.CorruptedCorvorite, 1);
		MainHatch.IsLocked = false;
	}

	public void ActivateSpotlight(bool activate) => screenEffects.SetBlit(spotlightEffectController.spotlightMaterial, activate);

	public void ActivateScriptedDrops(bool activate) => FirstQuestScriptedDrops.scriptedDropsActive = activate;

	public void SetShuttleRepaired(bool repaired) => ShuttleRepaired = repaired;

	public void SetShipRecharged(bool recharged) => ShipRecharged = recharged;

	public void StartQuestionHowToObtainEnergySourceDialogueAfterDelay(float delay = 3f)
	{
		Pause.DelayedAction(() =>
		{
			StartDialogue(questionHowToObtainEnergySourceDialogue, true);
		}, delay, true);
	}

	public void TakeItem(Item.Type type, int amount) => MainChar.TakeItem(type, amount);

	private void GiveQuest(Quester quester, Quest q) => quester.AcceptQuest(q);

	public void StartDialogue(ConversationWithActions ce, bool chat = false)
	{
		DlgCtrl.SkipEntireChat(true);

		if (chat)
		{
			DlgCtrl.StartChat(ce);
		}
		else
		{
			DlgCtrl.StartDialogue(ce);
		}
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
}
