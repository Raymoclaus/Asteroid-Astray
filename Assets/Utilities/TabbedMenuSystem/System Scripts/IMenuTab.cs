using System;
using UnityEngine;

public interface IMenuTab
{
	event Action<IMenuTab> OnClicked;
	string TabText { get; set; }
	int DrawOrder { get; set; }
	IMenuTab CreateCopy(Transform parent);
}