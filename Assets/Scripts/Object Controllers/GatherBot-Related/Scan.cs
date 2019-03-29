public struct Scan
{
	public EntityType type;
	public float hpRatio;
	public int level;
	public int value;

	public Scan(EntityType type, float hpRatio, int level, int value)
	{
		this.type = type;
		this.hpRatio = hpRatio;
		this.level = level;
		this.value = value;
	}
}