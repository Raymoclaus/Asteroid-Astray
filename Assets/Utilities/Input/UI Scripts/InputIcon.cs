﻿using System;
using UnityEngine;

namespace InputHandlerSystem.UI
{
	[Serializable]
	public struct InputIcon
	{
		public Sprite sprite;
		public InputCode input;

		public InputIcon(Sprite sprite, string ignoreSubstring = "")
		{
			this.sprite = sprite;
			input = new InputCode(InputCode.InputType.Button);
			Enum.TryParse(sprite.name.Replace(ignoreSubstring, string.Empty), out input.buttonCode);
		}
	}
}