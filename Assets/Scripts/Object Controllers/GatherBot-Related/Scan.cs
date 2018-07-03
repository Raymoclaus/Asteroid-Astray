public struct Scan
{
	public EntityType type;
	public float hpRatio;
	public int level;

	public Scan(EntityType t, float hpR, int lv)
	{
		type = t;
		hpRatio = hpR;
		level = lv;
	}
}