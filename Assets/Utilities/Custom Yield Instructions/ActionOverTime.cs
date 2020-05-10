using System;
using UnityEngine;

namespace CustomYieldInstructions
{
	public class ActionOverTime : CustomYieldInstruction
	{
		private Action<float> action;
		private float startTime, duration;
		private bool unscaledTime;

		public ActionOverTime(float duration, Action<float> action, bool unscaledTime)
		{
			startTime = unscaledTime ? Time.unscaledTime : Time.time;
			this.duration = duration;
			this.action = action;
			this.unscaledTime = unscaledTime;
		}

		public override bool keepWaiting
		{
			get
			{
				float currentTime = 0f;
				if (unscaledTime)
				{
					currentTime = Time.unscaledTime - startTime;
				}
				else
				{
					currentTime = Time.time - startTime;
				}
				float delta = Mathf.Clamp01(currentTime / duration);
				action?.Invoke(delta);
				return delta < 1f;
			}
		}
	}

}