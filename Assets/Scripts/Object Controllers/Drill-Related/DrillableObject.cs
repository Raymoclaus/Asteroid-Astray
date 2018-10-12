using UnityEngine;

public interface IDrillableObject
{
	bool TakeDrillDamage(float drillDmg, Vector2 drillPos, Entity destroyer, int dropModifier = 0);
	void StartDrilling();
	void StopDrilling();
	void OnTriggerEnter2D(Collider2D other);
	void OnTriggerExit2D(Collider2D other);
	void Launch(Vector2 launchDirection, Entity launcher);
	bool IsDrillable();
}