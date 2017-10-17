using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cnsts
{
	//mostly used for accuracy of collisions, higher numbers = less accurate but better performance
	public const float ACCURACY = 0.01f;
	//whenever infinite loops are a possibility (even by accident) try to remember to use this to break out of loops
	public const int FREEZE_LIMIT = 1000;
	//the size of chunks for groups of asteroids. Try not to use smaller than 10 if keeping default camera size
	public const int CHUNK_SIZE = 10;
}
