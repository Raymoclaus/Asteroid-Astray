using System;
using UnityEngine;

namespace TriggerSystem
{
	public interface IActor : IUnique
	{
		void EnteredTrigger(ITrigger vTrigger);
		void ExitedTrigger(ITrigger vTrigger);
		bool CanTriggerPrompts { get; }
		Vector3 Position { get; }
		event Action<IActor> OnDisabled;
	}
}