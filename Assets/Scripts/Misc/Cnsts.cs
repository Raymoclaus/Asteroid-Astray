using UnityEngine;

public static class Constants
{
	//the size of chunks for groups of asteroids. Try not to use smaller than 10 if keeping default camera size
	public const int CHUNK_SIZE = 10;
	//Entities that are too far from the main camera will have their physics disabled
	public const int MAX_PHYSICS_RANGE = 5;
}