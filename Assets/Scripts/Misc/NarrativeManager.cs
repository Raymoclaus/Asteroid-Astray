using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	[SerializeField] private DialogueController dialogueController;
	[SerializeField] private LoadingController loadingController;
	[SerializeField] private DebugGameplayManager debugGameplayManager;
	[SerializeField] private SpotlightEffectController spotlightEffectController;
	
	[SerializeField] private ConversationEvent recoveryDialogue;

	private void Start()
	{
		if (!debugGameplayManager.skipIntro)
		{
			loadingController.AddPostLoadAction(StartRecoveryDialogue);
		}
		else
		{
			if (spotlightEffectController)
			{
				spotlightEffectController.SetSpotlight();
			}
		}
	}

	private void StartRecoveryDialogue()
	{
		dialogueController.StartChat(recoveryDialogue);
	}
}
