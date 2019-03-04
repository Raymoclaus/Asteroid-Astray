using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	[SerializeField] private DialogueController dialogueController;
	[SerializeField] private LoadingController loadingController;
	[SerializeField] private DebugGameplayManager debugGameplayManager;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
	[SerializeField] private ShuttleTrackers shuttleTrackerSO;
	
	[SerializeField] private ConversationEvent recoveryDialogue;
	[SerializeField] private Character mainChar;

	[Header("Entity Profiles")]
	[SerializeField] private EntityProfile claire;

	private void Start()
	{
		if (!debugGameplayManager.skipIntro)
		{
			loadingController.AddPostLoadAction(StartRecoveryDialogue);
			screenEffects.SetBlit(spotlightEffectController.spotlightMaterial, true);
		}
		else
		{
			screenEffects.SetBlit(spotlightEffectController.spotlightMaterial, false);
			spotlightEffectController.SetSpotlight();
			loadingController.AddPostLoadAction(StartRecoveryQuest);
		}
	}

	private void StartRecoveryDialogue()
	{
		recoveryDialogue.conversationEndAction.AddListener(StartRecoveryQuest);
		dialogueController.StartDialogue(recoveryDialogue);
	}

	public void StartRecoveryQuest()
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
			mainChar, claire, qRewards, qReqs, StartFirstGatheringQuest);

		GiveQuest(mainChar, q);
	}

	public void StartFirstGatheringQuest(Quest other)
	{
		if (mainChar == null) return;

		List<QuestReward> qRewards = new List<QuestReward>();

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new CraftingQReq(Item.Type.RepairKit, 1, "Craft # ?"));

		Quest q = new Quest(
			"Craft Your First Repair Kit",
			"Now that we have the necessary materials, we should try crafting a repair kit.",
			mainChar, claire, qRewards, qReqs);

		GiveQuest(mainChar, q);
	}

	public void GiveQuest(Character c, Quest q)
	{
		c.AcceptQuest(q);
	}
}
