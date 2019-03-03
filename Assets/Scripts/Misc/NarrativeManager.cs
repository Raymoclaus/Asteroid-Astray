using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	[SerializeField] private DialogueController dialogueController;
	[SerializeField] private LoadingController loadingController;
	[SerializeField] private DebugGameplayManager debugGameplayManager;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	[SerializeField] private CustomScreenEffect screenEffects;
	
	[SerializeField] private ConversationEvent recoveryDialogue;
	[SerializeField] private Character mainChar;

	[Header("Entity Profiles")]
	[SerializeField] private EntityProfile claire;
	
	private const string qName_RepairCommunications = "Repair Communications";

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
		qRewards.Add(new ItemQReward(Item.Type.Copper, 10));

		List<QuestRequirement> qReqs = new List<QuestRequirement>();
		qReqs.Add(new GatheringQRec(Item.Type.Stone, 5, "Obtain # ? from asteroids"));

		Quest q = new Quest("Repair Communications", "We need some materials so that we can repair" +
			" our communications system. Once that is done, we should be able to find our way back" +
			" to Dendro and the ship.", mainChar, claire, qRewards, qReqs);
		//mainChar.AcceptQuest(q);
	}
}
