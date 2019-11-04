using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IElementHider))]
public class UIGroupHider : MonoBehaviour
{
	[SerializeField] private List<UIMoveTrigger> elements;

	private HashSet<IElementHider> elementHiders = new HashSet<IElementHider>();

	private void Start()
	{
		foreach (IElementHider hider in GetComponents<IElementHider>())
		{
			AddElementHider(hider);
		}
	}

	public void AddElementHider(IElementHider hider)
	{
		elementHiders.Add(hider);
		hider.OnActivate += Hide;
		hider.OnDeactivate += Show;
	}

	public void RemoveElementHider(IElementHider hider)
	{
		elementHiders.Remove(hider);
		hider.OnActivate -= Hide;
		hider.OnDeactivate -= Show;
	}

	public void Show(IElementHider hider)
	{
		Trigger(hider, t => t.Move(true));
	}

	public void Hide(IElementHider hider)
	{
		Trigger(hider, t => t.Move(false));
	}

	public void ShowInstantly(IElementHider hider)
	{
		Trigger(hider, t => t.InstantMove(true));
	}

	public void HideInstantly(IElementHider hider)
	{
		Trigger(hider, t => t.InstantMove(false));
	}

	private void Trigger(IElementHider hider, Action<UIMoveTrigger> action)
	{
		if (hider == null)
		{
			RemoveElementHider(hider);
			return;
		}
		hider.GroupHider.elements.ForEach(action);
	}
}
