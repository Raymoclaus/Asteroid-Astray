using UnityEngine;

public class TimeManager : MonoBehaviour
{
	public static GTime GameTime; 
	public static bool Running = false;

	private void Update()
	{
		if (Running)
		{
			GameTime.Add(Time.deltaTime);
		}
	}
}

public struct GTime
{
	public const float SecondsPerDay = 1800f;
	public int day;
	public float time;

	public GTime(int d = 1, float t = 420f)
	{
		day = d;
		time = t;
	}

	public void Add(float seconds)
	{
		time += seconds;
		while (seconds > SecondsPerDay)
		{
			day++;
			seconds -= SecondsPerDay;
		}
	}

	public static float SecondsBetween(GTime a, GTime b)
	{
		float seconds = 0f;
		do
		{
			if (a.day == b.day)
			{
				seconds += b.time - a.time;
				return seconds;
			}
			if (a.day < b.day)
			{
				a.day++;
				seconds += SecondsPerDay;
			}
			if (a.day > b.day)
			{
				a.day--;
				seconds -= SecondsPerDay;
			}
		} while (a.day != b.day);
		return b.time - a.time;
	}

	public static GTime Max(GTime a, GTime b)
	{
		if (a.day > b.day)
		{
			return a;
		}
		if (a.day < b.day)
		{
			return b;
		}
		if (a.time > b.time)
		{
			return a;
		}
		return b;
	}

	public static GTime Min(GTime a, GTime b)
	{
		if (a.day > b.day)
		{
			return b;
		}
		if (a.day < b.day)
		{
			return a;
		}
		if (a.time > b.time)
		{
			return b;
		}
		return a;
	}
}