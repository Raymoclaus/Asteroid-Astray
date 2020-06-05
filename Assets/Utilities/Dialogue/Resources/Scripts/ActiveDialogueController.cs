using DialogueSystem.UI;
using InputHandlerSystem;
using System;
using UnityEngine;

namespace DialogueSystem
{
	public class ActiveDialogueController : GameDialogueController
	{
		public static ActiveDialogueController _instance;

		public bool pause = true;
		[SerializeField] private InputContext _activeDialogueContext;
		private InputContext _previousContext;
		public InputContext ContextAfterDialogueEnds { get; set; }

		protected override void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			base.Awake();
			FindObjectOfType<ActiveDialoguePopupUI>()?.SetDialogueController(this);
		}
		
		public override void StartDialogue(ConversationWithActions newConversation)
		{
			if (pause)
			{
				TimeController.SetTimeScale(this, 0f);
			}

			_previousContext = InputManager.GetCurrentContext();
			InputManager.SetCurrentContext(_activeDialogueContext);

			if (PassiveDialogueController._instance?.DialogueIsRunning ?? false)
			{
				PassiveDialogueController._instance.Skip();
			}

			base.StartDialogue(newConversation);
		}

		public static void StartConversation(ConversationWithActions newConversation)
		{
			_instance.StartDialogue(newConversation);
		}

		protected override void EndDialogue()
		{
			if (pause)
			{
				TimeController.SetTimeScale(this, 1f);
			}

			InputContext nextContext = _previousContext;
			if (ContextAfterDialogueEnds != null)
			{
				nextContext = ContextAfterDialogueEnds;
				ContextAfterDialogueEnds = null;
			}
			InputManager.SetCurrentContext(nextContext);

			base.EndDialogue();
		}

		protected override float CharacterRevealSpeed => Time.unscaledDeltaTime;

		protected override void DelayedTextEvent(float delay, Action action)
		{
			Coroutines.UnscaledDelayedAction(delay, action);
		}
	}
}