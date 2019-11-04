using System;

public interface IElementHider
{
	UIGroupHider GroupHider { get; }
	event Action<IElementHider> OnActivate, OnDeactivate;
}
