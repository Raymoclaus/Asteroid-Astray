using System.Collections;
using UnityEngine;
using System;

public static class DelayedAction
{
	public static IEnumerator Go(Action a, WaitForSeconds time = null)
	{
		yield return time;
		a?.Invoke();
	}
}
