using System.Collections.Generic;

public class OneShotEventGroupWait : OneShotEvent
{
	private List<OneShotEvent> _eventsToWaitFor = new List<OneShotEvent>();

	private int TotalNumberOfEventsToWaitFor => _eventsToWaitFor.Count;

	private int NumberOfEventsCompleted { get; set; }

	public float Progress => (float)NumberOfEventsCompleted / TotalNumberOfEventsToWaitFor;

	private bool Started { get; set; }

	public OneShotEventGroupWait(bool startImmediately, params OneShotEvent[] events)
	{
		AddEventToWaitFor(events);

		if (startImmediately)
		{
			Start();
		}
	}

	public void AddEventToWaitFor(params OneShotEvent[] events)
	{
		_eventsToWaitFor.AddRange(events);
	}

	private void Add()
	{
		NumberOfEventsCompleted++;

		if (NumberOfEventsCompleted >= TotalNumberOfEventsToWaitFor)
		{
			Invoke();
		}
	}

	public void Start()
	{
		if (Started) return;
		Started = true;

		foreach (OneShotEvent iwe in _eventsToWaitFor)
		{
			iwe.RunWhenReady(Add);
		}
	}
}
