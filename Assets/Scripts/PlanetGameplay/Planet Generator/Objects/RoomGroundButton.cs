using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomGroundButton : RoomObject
{
	public event Action OnButtonTriggered;
	public event Action OnButtonReleased;
	private List<ActionOnTimer> heldActions = new List<ActionOnTimer>();

	private bool triggered = false;

	public RoomGroundButton(Room room) : base(room) { }

	public RoomGroundButton(Room room, string[] lines) : base(room, lines) { }

	public void Trigger(MonoBehaviour mono)
	{
		if (triggered) return;
		triggered = true;
		mono?.StartCoroutine(RunThroughHeldActions());
		OnButtonTriggered?.Invoke();
	}

	public void Release()
	{
		triggered = false;
		OnButtonReleased?.Invoke();
	}

	public void SubscribeToHeldEvent(Action action, float wait)
	{
		heldActions.Add(new ActionOnTimer(action, wait));
	}

	private IEnumerator RunThroughHeldActions()
	{
		while (triggered)
		{
			for (int i = 0; i < heldActions.Count; i++)
			{
				float delta = Time.deltaTime;
				heldActions[i].AddTime(delta);
			}
			yield return null;
		}

		for (int i = 0; i < heldActions.Count; i++)
		{
			heldActions[i].Reset();
		}
	}

	private class ActionOnTimer
	{
		private Action action;
		private float waitTime;
		private float timer;

		public ActionOnTimer(Action action, float waitTime)
		{
			this.action = action;
			this.waitTime = waitTime;
			timer = 0f;
		}

		public void AddTime(float add)
		{
			timer += add;
			if (timer >= waitTime)
			{
				timer -= waitTime;
				action?.Invoke();
			}
		}

		public float GetTimer() => timer;

		public void Reset() => timer = 0f;
	}
}
