using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	#region Narrative booleans
	public static bool ShuttleRepaired { get; private set; }
	public static bool ShipRecharged { get; private set; }
	#endregion

	private Pause pauseController;
	private Pause PauseController
	{
		get
		{
			return pauseController ?? (pauseController = FindObjectOfType<Pause>());
		}
	}
	[SerializeField] private DialogueController dialogueController;
	[SerializeField] private LoadingController loadingController;
	[SerializeField] private DebugGameplayManager dgm;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
	[SerializeField] private ShuttleTrackers shuttleTrackerSO;
	[SerializeField] private Character mainChar;
	[SerializeField] private MainHatchPrompt mainShip;
	[SerializeField] private DerangedSoloBot derangedSoloBotPrefab;

	[SerializeField] private ConversationEvent
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

	private void Start()
	{
		//ChooseStartingLocation();

		shuttleTrackerSO.SetControllable(false);
		shuttleTrackerSO.SetKinematic(true);
		mainShip.Lock(true);
		loadingController.AddPostLoadAction(() =>
		{
			shuttleTrackerSO.SetControllable(true);
			shuttleTrackerSO.SetKinematic(false);
		});

		if (dgm.skipRecoveryDialogue)
		{
			screenEffects.SetBlit(spotlightEffectController.spotlightMaterial, false);
			spotlightEffectController.SetSpotlight();
			if (dgm.skipFirstGatheringQuest)
			{
				if (dgm.skipMakeARepairKitQuest)
				{
					if (dgm.skipRepairTheShuttleQuest)
					{
						ShuttleRepaired = true;
						loadingController.AddPostLoadAction(() =>
						{
							shuttleTrackerSO.SetNavigationActive(true);
						});
						if (dgm.skipReturnToTheShipQuest)
						{
							if (dgm.skipAquireAnEnergySourceQuest)
							{
								loadingController.AddPostLoadAction(() =>
								{
									mainChar.CollectResources(Item.Type.CorruptedCorvorite, 1);
									CompletedFindEnergySourceQuest(null);
								});
								return;
							}
							loadingController.AddPostLoadAction(() =>
							{
								CompletedReturnToTheShipQuest(null);
							});
							return;
						}
						loadingController.AddPostLoadAction(() =>
						{
							CompletedRepairTheShuttleQuest(null);
						});
						return;
					}
					loadingController.AddPostLoadAction(() =>
					{
						mainChar.CollectResources(Item.Type.RepairKit, 1);
						CompletedCraftYourFirstRepairKitQuest(null);
					});
					return;
				}
				loadingController.AddPostLoadAction(() =>
				{
					mainChar.CollectResources(Item.Type.Copper, 2);
					mainChar.CollectResources(Item.Type.Iron, 1);
					CompletedFirstGatheringQuest(null);
				});
				return;
			}
			loadingController.AddPostLoadAction(StartFirstGatheringQuest);
			return;
		}
		loadingController.AddPostLoadAction(StartRecoveryDialogue);
	}

	private void StartRecoveryDialogue()
	{
		StartDialogue(recoveryDialogue, false, StartFirstGatheringQuest);
		screenEffects.SetBlit(spotlightEffectController.spotlightMaterial, true);
	}

	private void StartFirstGatheringQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQRec(Item.Type.Copper, 2, "Obtain # ? from asteroids."));
		qReqs.Add(new GatheringQRec(Item.Type.Iron, 1, "Obtain # ? from asteroids."));

		Quest q = new Quest(
			"Gather materials",
			"We need some materials so that we can repair our communications system. Once that" +
			" is done, we should be able to find our way back to Dendro and the ship.",
			mainChar, claire, qRewards, qReqs, CompletedFirstGatheringQuest);

		GiveQuest(mainChar, q);
		FirstQuestScriptedDrops.scriptedDropsActive = true;
		StartDialogue(UseThrustersDialogue, true);
	}

	private void CompletedFirstGatheringQuest(Quest quest)
	{
		StartDialogue(completedFirstGatheringQuestDialogue, true);
		StartCraftYourFirstRepairKitQuest();
		FirstQuestScriptedDrops.scriptedDropsActive = false;
	}

	private void StartCraftYourFirstRepairKitQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new CraftingQReq(Item.Type.RepairKit, 1, "Craft # ? using 2 copper and 1 iron."));

		Quest q = new Quest(
			"Craft Your First Repair Kit",
			"Now that we have the necessary materials, we should try crafting a repair kit.",
			mainChar, claire, qRewards, qReqs, CompletedCraftYourFirstRepairKitQuest);

		GiveQuest(mainChar, q);
	}

	private void CompletedCraftYourFirstRepairKitQuest(Quest quest)
	{
		StartRepairTheShuttleQuest();
		StartDialogue(useRepairKitDialogue, true, null);
	}

	private void StartRepairTheShuttleQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new ItemUseQReq(Item.Type.RepairKit, 1, "Use # ?."));

		Quest q = new Quest(
			"Repair the Shuttle",
			"Using this repair kit should be enough to fix the communications system. Then we can finally get back to the ship.",
			mainChar, claire, qRewards, qReqs, CompletedRepairTheShuttleQuest);

		GiveQuest(mainChar, q);
	}

	private void CompletedRepairTheShuttleQuest(Quest quest)
	{
		shuttleTrackerSO.SetNavigationActive(true);
		ShuttleRepaired = true;
		StartDialogue(findShipDialogue, true, null);
		StartReturnToTheShipQuest();
	}

	private void StartReturnToTheShipQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new InteractionQReq(mainShip, "Return to the ship."));

		Quest q = new Quest(
			"Find your way back to the ship",
			"Communication and Navigation systems on the shuttle have been restored, but we still can't contact Dendro. Find your way back to the ship and check if Dendro is still alright.",
			mainChar, claire, qRewards, qReqs, CompletedReturnToTheShipQuest);

		GiveQuest(mainChar, q);
		mainShip.EnableDialogueResponses(false);
	}

	private void CompletedReturnToTheShipQuest(Quest quest)
	{		
		StartDialogue(foundShipDialogue, false, StartFindEnergySourceQuest);
		mainShip.EnableDialogueResponses(true);
	}

	private void StartFindEnergySourceQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQRec(Item.Type.CorruptedCorvorite, 1, "Acquire an energy source.", false));

		Quest q = new Quest(
			"Acquire an Energy Source",
			"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
			mainChar, claire, qRewards, qReqs, CompletedFindEnergySourceQuest);

		GiveQuest(mainChar, q);
		//create a deranged bot
		Entity newEntity = EntityGenerator.SpawnEntity(derangedSoloBotPrefab);
		//set waypoint to new bot
		shuttleTrackerSO.SetWaypoint(newEntity.transform, null);
		//attach dialogue prompt when player approaches bot
		VicinityTrigger entityPrompt = newEntity.GetComponentInChildren<VicinityTrigger>();
		VicinityTrigger.VicinityTriggerEventHandler triggerEnterAction = null;
		triggerEnterAction = () =>
		{
			UnityEngine.Events.UnityAction delayedDialogue = () =>
			{
				PauseController.DelayedAction(() =>
				{
					if (newEntity.GetHpRatio() < 0.9f) return;
					StartDialogue(questionHowToObtainEnergySourceDialogue, true);
				}, 5f, true);
			};
			StartDialogue(foundDerangedBotDialogue, true, delayedDialogue);
			entityPrompt.OnEnterTrigger -= triggerEnterAction;
		};
		entityPrompt.OnEnterTrigger += triggerEnterAction;
	}

	private void CompletedFindEnergySourceQuest(Quest quest)
	{
		StartDialogue(acquiredEnergySourceDialogue, true);
		StartRechargeTheShipQuest();
	}

	private void StartRechargeTheShipQuest()
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new WaypointQReq(mainShip.transform, "Return to the ship."));

		Quest q = new Quest(
			"Recharge the Ship",
			"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
			mainChar, claire, qRewards, qReqs, CompletedRechargeTheShipQuest);

		GiveQuest(mainChar, q);
	}

	private void CompletedRechargeTheShipQuest(Quest quest)
	{
		ShipRecharged = true;
		StartDialogue(rechargedTheShipDialogue);
		mainChar.TakeItem(Item.Type.CorruptedCorvorite, 1);
		mainShip.Lock(false);
	}

	private void GiveQuest(Character c, Quest q) => c.AcceptQuest(q);

	private void StartDialogue(ConversationEvent ce, bool chat = false,
		UnityEngine.Events.UnityAction action = null)
	{
		dialogueController.SkipEntireChat(true);

		if (action != null)
		{
			ce.conversationEndAction.AddListener(action);
		}

		if (chat)
		{
			dialogueController.StartChat(ce);
		}
		else
		{
			dialogueController.StartDialogue(ce);
		}
	}

	private void ChooseStartingLocation()
	{
		Vector2 pos = mainShip.transform.position;
		float randomAngle = UnityEngine.Random.value * Mathf.PI * 2f;
		Vector2 randomPos = new Vector2(Mathf.Sin(randomAngle),	Mathf.Cos(randomAngle));
		float div = DistanceUI.UNITS_TO_METRES;
		randomPos *= UnityEngine.Random.value * 100f / div + 300f / div;
		mainChar.Teleport(pos + randomPos);
	}
}
