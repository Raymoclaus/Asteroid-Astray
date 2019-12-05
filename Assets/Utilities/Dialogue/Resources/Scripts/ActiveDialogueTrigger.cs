using System;
using System.Collections;
using System.Collections.Generic;
using DialogueSystem;
using TriggerSystem.Actors.Interactors;
using TriggerSystem.Triggers;
using UnityEngine;

namespace TriggerSystem.MessageReceivers
{
	[RequireComponent(typeof(ChatBehaviour))]
	public class ActiveDialogueTrigger : MonoBehaviour, IChatter
	{
		[SerializeField] private InteractionTrigger triggerComponent;
		public ConversationWithActions conversation;

		[SerializeField] private bool allowDialogue = true;
		public bool CanSendDialogue => allowDialogue;

		public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
		public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;

		private void Awake()
		{
			triggerComponent.OnInteracted += ShowDialogue;
		}

		private void ShowDialogue(IInteractor obj)
		{
			OnSendActiveDialogue?.Invoke(conversation, false);
		}

		public void AllowSendingDialogue(bool allow)
		{
			allowDialogue = allow;
		}
	}
}