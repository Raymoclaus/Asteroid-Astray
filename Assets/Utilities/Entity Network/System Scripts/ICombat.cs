public interface ICombat
{
	bool EngageInCombat(ICombat hostile);
	void DisengageInCombat(ICombat nonHostile);
}
