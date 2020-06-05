using System;

public abstract class OneShotEvent
{
	private event Action _event;

	public bool Invoked { get; private set; }

	public void RunWhenReady(Action a)
	{
		if (Invoked)
		{
			a?.Invoke();
		}
		else
		{
			_event += a;
		}
	}

	public void RemoveListener(Action a)
	{
		_event -= a;
	}

	protected void Invoke()
	{
		if (Invoked) return;
		Invoked = true;
		_event?.Invoke();
		_event = null;
	}
}
