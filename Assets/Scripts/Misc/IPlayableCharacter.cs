using System;

public interface IPlayableCharacter
{
	Character GetCharacter();
	event Action OnGoInput, OnLaunchInput;
	event Action<bool> OnDrillComplete;
	bool HasControl { get; }
	bool IsDrilling { get; }
	bool CanDrillLaunch { get; }
	bool CanShoot { get; }
	bool CanBoost { get; }
	float BoostPercentage { get; }
}
