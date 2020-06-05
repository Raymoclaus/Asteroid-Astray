﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PopupUI : MonoBehaviour
{
	[SerializeField]
	protected int popupViewLimit = 4;
	[SerializeField]
	protected float popupEntrySpeed = 2f, popupMoveSpeed = 5f;
	protected HashSet<PopupObject> activePopups = new HashSet<PopupObject>();
	protected HashSet<PopupObject> inactivePopups = new HashSet<PopupObject>();

	protected int ActivePopupCount => activePopups.Count;

	protected bool ViewingLimitReached => ActivePopupCount >= popupViewLimit;

	protected bool ContainsActivePopup => ActivePopupCount > 0;

	protected int HighestID => ActivePopupCount > 0 ? activePopups.Max(t => t.ID) : -1;

	protected void RemoveLastPopup()
	{
		int maxID = HighestID;
		RemovePopupsWithID(maxID);
	}

	protected PopupObject LastPopup
		=> GetActivePopupWithID(activePopups.Max(t => t.ID));

	protected PopupObject FirstPopup
		=> GetActivePopupWithID(activePopups.Min(t => t.ID));

	protected void RemovePopupsWithID(int ID)
	{
		RemoveMatchingPopups(t => t.ID == ID);
	}

	protected virtual void RemovePopup(PopupObject po)
	{
		if (!activePopups.Contains(po) || inactivePopups.Contains(po)) return;

		inactivePopups.Add(po);
		activePopups.Remove(po);

		int ID = po.ID;
		int maxID = HighestID;
		for (int i = ID + 1; i <= maxID; i++)
		{
			DecrementPopupsWithID(i);
		}
	}

	private void DecrementPopupsWithID(int ID)
	{
		foreach (PopupObject po in activePopups.Where(t => t.ID == ID))
		{
			po.ID--;
		}
	}

	protected void RemoveMatchingPopups(Func<PopupObject, bool> pattern)
	{
		foreach (PopupObject po in activePopups.Where(pattern).ToList())
		{
			RemovePopup(po);
		}
	}

	protected void RemovePopupUsingTransform(RectTransform transform)
	{
		RemoveMatchingPopups(t => t.transform == transform);
	}

	protected void RemovePopupsWithTimerGreaterThanOrEqualToTime(float time)
	{
		RemoveMatchingPopups(t => t.Timer >= time);
	}

	protected void RemoveAllPopups()
	{
		while (ContainsActivePopup)
		{
			RemoveLastPopup();
		}
	}

	protected PopupObject GetActivePopupWithID(int ID)
	{
		return activePopups.FirstOrDefault(t => t.ID == ID);
	}

	protected virtual PopupObject GetAnInactivePopup => inactivePopups.FirstOrDefault();

	protected void ActivatePopup(PopupObject po)
	{
		if (activePopups.Contains(po) || !inactivePopups.Contains(po)) return;
		foreach (PopupObject activePo in activePopups)
		{
			activePo.ID++;
		}

		po.ID = 0;
		activePopups.Add(po);
		inactivePopups.Remove(po);
	}

	protected float GetTargetHeight(PopupObject po)
	{
		float count = 0f;
		for (int i = 0; i < po.ID; i++)
		{
			PopupObject other = GetActivePopupWithID(i);
			count += other.Height;
		}

		return count;
	}

	protected class PopupObject
	{
		public float Timer { get; private set; }
		public RectTransform transform;
		public int ID { get; set; }

		public PopupObject(RectTransform transform)
		{
			this.transform = transform;
		}

		public void ResetTimer() => SetTimer(0f);

		public void SetTimer(float time)
		{
			Timer = time;
		}

		public void AddTimer(float time)
		{
			Timer += time;
		}

		public float Height => transform.rect.height;
	}
}
