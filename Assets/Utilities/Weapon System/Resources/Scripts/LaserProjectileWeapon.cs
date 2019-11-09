using UnityEngine;

namespace WeaponSystem
{
	public class LaserProjectileWeapon : BasicProjectileWeapon
	{
		[SerializeField] private string triggerAction = "Attack";
		[SerializeField] private Transform weaponPivot;

		public override string TriggerAction => triggerAction;

		protected override Vector3 WeaponPosition => weaponPivot.position;
	} 
}
