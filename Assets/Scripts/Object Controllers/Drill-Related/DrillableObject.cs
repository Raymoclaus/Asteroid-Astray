using UnityEngine;

public interface IDrillableObject
{
	bool TakeDrillDamage(float drillDmg);
	void StartDrilling();
	void StopDrilling();
	void OnTriggerEnter2D(Collider2D other);
	void OnTriggerExit2D(Collider2D other);
}