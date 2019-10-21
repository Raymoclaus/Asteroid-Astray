using System;
using UnityEngine;

namespace CustomYieldInstructions
{
	public class ActionOverTime : CustomYieldInstruction
	{
		private Action<float> action;
		private float startTime, duration;

		public ActionOverTime(float duration, Action<float> action)
		{
			startTime = Time.time;
			this.duration = duration;
			this.action = action;
		}

		public override bool keepWaiting
		{
			get
			{
				float currentTime = Time.time - startTime;
				float delta = Mathf.Clamp01(currentTime / duration);
				action?.Invoke(delta);
				return delta < 1f;
			}
		}
	}

}