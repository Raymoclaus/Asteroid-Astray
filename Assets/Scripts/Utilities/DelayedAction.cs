using System.Collections;
using UnityEngine;
using System;

public static class DelayedAction
{
	public static IEnumerator Go(Action a, float? time = null)
	{
		yield return time == null ? null : new WaitForSeconds((float)time);
		a();
	}
}
