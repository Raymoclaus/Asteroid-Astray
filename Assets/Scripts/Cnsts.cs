public static class Cnsts
{
	//whenever infinite loops are a possibility (even by accident) try to remember to use this to break out of loops
	public const int FreezeLimit = 1000;

	//the size of chunks for groups of asteroids. Try not to use smaller than 10 if keeping default camera size
	public const int ChunkSize = 10;

	//globally controls how fast everything moves relative to time while still being bound to frame rate
	public const float TimeSpeed = 1f;
}