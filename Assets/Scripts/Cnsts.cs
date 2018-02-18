using UnityEngine;

public class Cnsts : MonoBehaviour
{
	public static Cnsts cnsts;
	
	//the size of chunks for groups of asteroids. Try not to use smaller than 10 if keeping default camera size
	public int ChunkSize = 10;
	//globally controls how fast everything moves relative to time while still being bound to frame rate
	public float TimeSpeed = 1f;
	//Entities that are too far from the main camera will have their physics disabled
	public int MaxPhysicsRange = 3;

	public static int CHUNK_SIZE { get { return cnsts == null ? 10 : cnsts.ChunkSize; } }
	public static float TIME_SPEED { get { return cnsts == null ? 1f : cnsts.TimeSpeed; } }
	public static int MAX_PHYSICS_RANGE { get { return cnsts == null ? 3 : cnsts.MaxPhysicsRange; } }

	private void Awake()
	{
		if (cnsts == null)
		{
			cnsts = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (cnsts != this)
		{
			Destroy(gameObject);
		}
	}

	public void UpTimeSpeed()
	{
		TimeSpeed += 0.5f;
	}

	public void DownTimeSpeed()
	{
		TimeSpeed -= 0.5f;
	}

	public void UpMaxPhysicsRange()
	{
		MaxPhysicsRange += 1;
	}

	public void DownMaxPhysicsRange()
	{
		MaxPhysicsRange -= 1;
	}
}