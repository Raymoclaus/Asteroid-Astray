[System.Serializable]
public class SpawnableEntity
{
	//reference to the prefab
	public Entity prefab;
	//name of the Entity type
	public string name = "default";
	//chance that the chunk will be filled with this entity type
	public float rarity;
	//in the event this entity type is spawned, how many should spawn in the chunk
	public IntPair spawnRange;
	//determines where in a chunk the entity should be spawned
	public SpawnPosition posType = SpawnPosition.Random;
	//convenient off switch for telling the entity generator to ignore this
	public bool ignore = false;

	public enum SpawnPosition
	{
		Random,
		Center
	}
}