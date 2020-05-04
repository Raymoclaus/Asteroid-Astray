using DialogueSystem.UI;
using UnityEngine;

namespace DialogueSystem
{
	public class PassiveDialogueController : GameDialogueController
	{
		public static PassiveDialogueController _instance;

		[SerializeField] private float autoScrollTime = 1.5f;
		private float autoScrollTimer;

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
			FindObjectOfType<PassiveDialoguePopupUI>()?.SetDialogueController(this);
		}

		protected override void Update()
		{
			base.Update();

			if (IsTyping || IsWaitingOnDelayedText)
			{
				autoScrollTimer = 0f;
			}
			else
			{
				autoScrollTimer += Time.deltaTime;
				if (autoScrollTimer >= autoScrollTime)
				{
					Next();
					autoScrollTimer = 0f;
				}
			}
		}

		public static void StartConversation(ConversationWithActions newConversation)
		{
			_instance.StartDialogue(newConversation);
		}
	}
}