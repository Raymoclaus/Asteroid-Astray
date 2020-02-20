using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace DialogueSystem
{
	[Serializable]
	public class ConversationWithActions
	{
		public ConversationEvent conversationEvent;
		public List<UnityEvent> events = new List<UnityEvent>();
		public int Length => conversationEvent?.conversation.Length ?? 0;
		public UnityEvent endEvent = new UnityEvent();

		public UnityEvent EndEvent => endEvent;

		public void AddAction(int index, Action action)
		{
			if (action == null) return;
			if (index >= Length) return;
			EnsureLength();
			events[index].AddListener(new UnityAction(action));
		}

		public void InvokeEvent(int index)
		{
			if (index >= Length) return;
			EnsureLength();
			events[index]?.Invoke();
		}

		public void InvokeEndEvent() => endEvent?.Invoke();

		public void AddActionToEnd(Action action)
		{
			if (action == null) return;
			endEvent.AddListener(new UnityAction(action));
		}

		public DialogueTextEvent[] Lines => conversationEvent.conversation;

		public DialogueTextEvent GetLine(int index)
			=> index >= 0 && index < Length
				? conversationEvent.conversation[index] : null;

		public CharacterProfile[] Speakers => conversationEvent.speakers;

		public ConversationEventPosition NextConversation
			=> conversationEvent.GetNextConversation();

		public void EnsureLength()
		{
			while (Length > events.Count)
			{
				events.Add(new UnityEvent());
			}
		}

		public float GetDelay(int index)
		{
			return conversationEvent.conversation[index].delay;
		}

		public void SetDelay(int index, float value)
		{
			conversationEvent.conversation[index].delay = value;
		}

		public bool GetAlsoDelayEvent(int index)
		{
			return conversationEvent.conversation[index].alsoDelayEventInvocation;
		}

		public void SetToAlsoDelayEvent(int index, bool value)
		{
			conversationEvent.conversation[index].alsoDelayEventInvocation = value;
		}
	}
}
