﻿using UnityEngine;

namespace TriggerSystem
{
	public interface ITrigger
	{
		int TriggerLayer { get; }
		bool TriggerEnabled { get; set; }
		Vector3 PivotPosition { get; }
	}

}