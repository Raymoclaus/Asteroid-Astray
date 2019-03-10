using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NarrativeManager : MonoBehaviour
{
	[SerializeField] private DialogueController dialogueController;
	[SerializeField] private LoadingController loadingController;
	[FormerlySerializedAs("debugGameplayManager")]
	[SerializeField] private DebugGameplayManager dgm;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
	[SerializeField] private ShuttleTrackers shuttleTrackerSO;
	
	[SerializeField] private ConversationEvent
		recoveryDialogue,
		UseThrustersDialogue,
		completedFirstGatheringQuestDialogue,
		useRepairKitDialogue,
		findShipDialogue;
	[SerializeField] private Character mainChar;

	[Header("Entity Profiles")]
	[SerializeField] private EntityProfile claire;

	public static bool ShipAvailable { get; private set; } = false;

	private void Start()
	{
		shuttleTrackerSO.SetControllable(true);
		shuttleTrackerSO.SetKinematic(false);

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
						loadingController.AddPostLoadAction(() =>
						{
							ShipAvailable = true;
							shuttleTrackerSO.SetNavigationActive(true);
						});
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
		qReqs.Add(new GatheringQRec(Item.Type.Copper, 2, "Obtain # ? from asteroids"));
		qReqs.Add(new GatheringQRec(Item.Type.Iron, 1, "Obtain # ? from asteroids"));

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
		StartCraftYourFirstRepairKitQuest(null);
		FirstQuestScriptedDrops.scriptedDropsActive = false;
	}

	private void StartCraftYourFirstRepairKitQuest(Quest other)
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new CraftingQReq(Item.Type.RepairKit, 1, "Craft # ?"));

		Quest q = new Quest(
			"Craft Your First Repair Kit",
			"Now that we have the necessary materials, we should try crafting a repair kit.",
			mainChar, claire, qRewards, qReqs, CompletedCraftYourFirstRepairKitQuest);

		GiveQuest(mainChar, q);
	}

	private void CompletedCraftYourFirstRepairKitQuest(Quest quest)
	{
		StartRepairTheShuttleQuest(null);
		StartDialogue(useRepairKitDialogue, true, null);
	}

	private void StartRepairTheShuttleQuest(Quest other)
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new ItemUseQReq(Item.Type.RepairKit, 1, "Use # ?"));

		Quest q = new Quest(
			"Repair the Shuttle",
			"Using this repair kit should be enough to fix the communications system. Then we can finally get back to the ship.",
			mainChar, claire, qRewards, qReqs, CompletedRepairTheShuttleQuest);

		GiveQuest(mainChar, q);
	}

	private void CompletedRepairTheShuttleQuest(Quest quest)
	{
		ShipAvailable = true;
		shuttleTrackerSO.SetNavigationActive(true);
		StartDialogue(findShipDialogue, true, null);
	}

	private void GiveQuest(Character c, Quest q) => c.AcceptQuest(q);

	private void StartDialogue(ConversationEvent ce, bool chat = false,
		UnityEngine.Events.UnityAction action = null)
	{
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
}
