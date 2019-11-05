using DialogueSystem.UI;
using UnityEngine;

namespace DialogueSystem
{
	public class PassiveDialogueController : GameDialogueController
	{
		[SerializeField] private float autoScrollTime = 1.5f;
		private float autoScrollTimer;

		protected override void Awake()
		{
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
	}
}