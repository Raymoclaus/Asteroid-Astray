﻿using System;
using UnityEngine;

namespace DialogueSystem
{
	[Serializable]
	public class DialogueTextEvent
	{
		[SerializeField]
		[TextArea(1, 2)]
		public string line;
		public byte speakerID;
		public const string DEFAULT_LINE = "<No dialogue line available>";
		public float delay;
		public bool alsoDelayEventInvocation;
		public float characterRevealSpeedMultiplier = 1f;
	}
}
