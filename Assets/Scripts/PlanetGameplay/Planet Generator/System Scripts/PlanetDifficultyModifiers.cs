[System.Serializable]
public struct PlanetDifficultyModifiers
{
	public float difficultySetting;
	public int minBranchLength, maxBranchLength;
	public int minBranchCount;
	public const int MAX_BRANCH_COUNT = 4;
	public int minDeadEndCount, maxDeadEndCount;
	public float enemyRoomDifficulty;

	public PlanetDifficultyModifiers(float difficultySetting)
	{
		this.difficultySetting = difficultySetting;
		minBranchLength = (int)(difficultySetting);
		maxBranchLength = (int)(difficultySetting * 2f);
		minBranchCount = (int)(difficultySetting / 2f);
		minDeadEndCount = 1;
		maxDeadEndCount = (int)(difficultySetting * 1.5f);
		enemyRoomDifficulty = difficultySetting * 2f;
	}
}