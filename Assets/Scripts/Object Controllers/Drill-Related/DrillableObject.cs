using UnityEngine;
using System.Collections.Generic;

public interface IDrillableObject
{
	bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0);
	void StartDrilling(DrillBit db);
	void StopDrilling(DrillBit db);
	void OnTriggerEnter2D(Collider2D other);
	void OnTriggerExit2D(Collider2D other);
	void Launch(Vector2 launchDirection, Character launcher);
	bool IsDrillable();
	bool CanBeLaunched();
	List<DrillBit> GetDrillers();
	void AddDriller(DrillBit db);
	bool RemoveDriller(DrillBit db);
}