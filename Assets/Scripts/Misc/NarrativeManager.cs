using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	#region Narrative booleans
	public static bool ShuttleRepaired { get; private set; }
	public static bool ShipRecharged { get; private set; }
	#endregion

	[SerializeField] private bool randomiseStartingLocation;
	private DialogueController dlgCtrl;
	private DialogueController DlgCtrl
		=> dlgCtrl ?? (dlgCtrl = FindObjectOfType<DialogueController>());
	[SerializeField] private DebugGameplayManager dgm;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
	[SerializeField] private Shuttle mainChar;
	private Shuttle MainChar => mainChar ?? (mainChar = FindObjectOfType<Shuttle>());
	[SerializeField] private Triggerer playerTriggerer;
	private Triggerer PlayerTriggerer
		=> playerTriggerer ?? (playerTriggerer = MainChar.GetComponent<Triggerer>());
	private MainHatchPrompt mainHatch;
	private MainHatchPrompt MainHatch
		=> mainHatch ?? (mainHatch = FindObjectOfType<MainHatchPrompt>());
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
		if (randomiseStartingLocation)
		{
			ChooseStartingLocation();
		}

		MainChar.hasControl = false;
		MainChar.isKinematic = true;
		MainHatch.Lock(true);
		MainChar.SetNavigationActive(false);
		LoadingController.AddListener(() =>
		{
			MainChar.hasControl = true;
			MainChar.isKinematic = false;
		});

		if (dgm.skipRecoveryDialogue)
		{
			ActivateSpotlight(false);
			spotlightEffectController.SetSpotlight();
			if (dgm.skipFirstGatheringQuest)
			{
				if (dgm.skipMakeARepairKitQuest)
				{
					if (dgm.skipRepairTheShuttleQuest)
					{
						ShuttleRepaired = true;
						LoadingController.AddListener(() =>
						{
							MainChar.SetNavigationActive(true);
						});
						if (dgm.skipReturnToTheShipQuest)
						{
							if (dgm.skipAquireAnEnergySourceQuest)
							{
								LoadingController.AddListener(() =>
								{
									MainChar.CollectItem(new ItemStack(Item.Type.CorruptedCorvorite, 1));
									CompletedFindEnergySourceQuest(null);
								});
								return;
							}
							LoadingController.AddListener(() =>
							{
								CompletedReturnToTheShipQuest(null);
							});
							return;
						}
						LoadingController.AddListener(() =>
						{
							CompletedRepairTheShuttleQuest(null);
						});
						return;
					}
					LoadingController.AddListener(() =>
					{
						MainChar.CollectItem(new ItemStack(Item.Type.RepairKit, 1));
						CompletedCraftYourFirstRepairKitQuest(null);
					});
					return;
				}
				LoadingController.AddListener(() =>
				{
					MainChar.CollectItem(new ItemStack(Item.Type.Copper, 2));
					MainChar.CollectItem(new ItemStack(Item.Type.Iron, 1));
					CompletedFirstGatheringQuest(null);
				});
				return;
			}
			LoadingController.AddListener(StartFirstGatheringQuest);
			return;
		}
		LoadingController.AddListener(StartRecoveryDialogue);
	}

	private void StartRecoveryDialogue() => StartDialogue(recoveryDialogue, false);

	public void StartFirstGatheringQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQRec(Item.Type.Copper, 2, "Obtain # ? from asteroids"));
		qReqs.Add(new GatheringQRec(Item.Type.Iron, 1, "Obtain # ? from asteroids"));

		Quest q = new Quest(
			"Gather materials",
			"We need some materials so that we can repair our communications system. Once that" +
			" is done, we should be able to find our way back to Dendro and the ship.",
			MainChar, claire, qRewards, qReqs, CompletedFirstGatheringQuest);

		GiveQuest(MainChar, q);
		FirstQuestScriptedDrops.scriptedDropsActive = true;
		StartDialogue(UseThrustersDialogue, true);
		TutPrompts?.drillInputPromptInfo.SetIgnore(false);
	}

	private void CompletedFirstGatheringQuest(Quest quest)
	{
		StartDialogue(completedFirstGatheringQuestDialogue, true);
		TutPrompts.drillInputPromptInfo.SetIgnore(true);
	}

	public void StartCraftYourFirstRepairKitQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new CraftingQReq(Item.Type.RepairKit, 1, "Construct # ? using 2 copper and 1 iron"));

		Quest q = new Quest(
			"Construct a Repair Kit",
			"Now that we have the necessary materials, we should try constructing a repair kit.",
			MainChar, claire, qRewards, qReqs, CompletedCraftYourFirstRepairKitQuest);

		GiveQuest(MainChar, q);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(false);

		CraftingRecipe? recipe = Crafting.GetRecipeByName("Repair Kit Recipe");
		if (recipe != null)
		{
			InventoryUI.SetGhostRecipe((CraftingRecipe)recipe);
		}
	}

	private void CompletedCraftYourFirstRepairKitQuest(Quest quest)
	{
		StartDialogue(useRepairKitDialogue, true);
		TutPrompts?.pauseInputPromptInfo.SetIgnore(true);
	}

	public void StartRepairTheShuttleQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new ItemUseQReq(Item.Type.RepairKit, 1, "Use # ?"));

		Quest q = new Quest(
			"Repair the Shuttle",
			"Using this repair kit should be enough to fix the communications system. Then we can finally get back to the ship.",
			MainChar, claire, qRewards, qReqs, CompletedRepairTheShuttleQuest);

		GiveQuest(MainChar, q);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(false);
	}

	private void CompletedRepairTheShuttleQuest(Quest quest)
	{
		StartDialogue(findShipDialogue, true);
		TutPrompts?.repairKitInputPromptInfo.SetIgnore(true);
	}

	public void StartReturnToTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new InteractionQReq(MainHatch, MainChar.GetComponent<Triggerer>(), "Return to the ship."));

		Quest q = new Quest(
			"Find your way back to the ship",
			"Communication and Navigation systems on the shuttle have been restored, but we still can't contact Dendro. Find your way back to the ship and check if Dendro is still alright.",
			MainChar, claire, qRewards, qReqs, CompletedReturnToTheShipQuest);

		GiveQuest(MainChar, q);
	}

	private void CompletedReturnToTheShipQuest(Quest quest) => StartDialogue(foundShipDialogue, false);

	public void StartFindEnergySourceQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQRec(Item.Type.CorruptedCorvorite, 1, "Find the nearby energy source.", false));

		Quest q = new Quest(
			"Acquire an Energy Source",
			"The ship appears intact, however it is in a powered-down state. We need to find an energy source.",
			MainChar, claire, qRewards, qReqs, CompletedFindEnergySourceQuest);

		GiveQuest(MainChar, q);
		//create a deranged bot
		Entity newEntity = EntityGenerator.SpawnEntity(derangedSoloBotPrefab);
		//set waypoint to new bot
		MainChar.waypoint = new Waypoint(newEntity.transform, null);
		//attach dialogue prompt when player approaches bot
		VicinityTrigger entityPrompt = newEntity.GetComponentInChildren<VicinityTrigger>();
		VicinityTrigger.EnteredTriggerEventHandler triggerEnterAction = null;
		triggerEnterAction = (Triggerer actor) =>
		{
			StartDialogue(foundDerangedBotDialogue, true);
			entityPrompt.OnEnterTrigger -= triggerEnterAction;
		};
		entityPrompt.OnEnterTrigger += triggerEnterAction;
	}

	private void CompletedFindEnergySourceQuest(Quest quest) => StartDialogue(acquiredEnergySourceDialogue, true);

	public void StartRechargeTheShipQuest()
	{
		if (MainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();

		Waypoint wp = new Waypoint(MainHatch.transform);
		MainChar.waypoint = wp;
		qReqs.Add(new WaypointQReq(wp, "Return to the ship."));

		Quest q = new Quest(
			"Recharge the Ship",
			"Now that we have an energy source, we should take it back to the ship and restore power so that we can finally get back inside.",
			MainChar, claire, qRewards, qReqs, CompletedRechargeTheShipQuest);

		GiveQuest(MainChar, q);
	}

	private void CompletedRechargeTheShipQuest(Quest quest)
	{
		StartDialogue(rechargedTheShipDialogue, false);
		TakeItem(Item.Type.CorruptedCorvorite, 1);
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

	private void GiveQuest(Character c, Quest q) => c.AcceptQuest(q);

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
		float div = DistanceUI.UNITS_TO_METRES;
		randomPos *= UnityEngine.Random.value * 100f / div + 300f / div;
		MainChar?.Teleport(pos + randomPos);
	}
}
