using UnityEngine;
using AttackData;

namespace WeaponSystem
{
	public interface IProjectileWeapon : IWeapon
	{
		new IAmmo GetAttack { get; }
		bool IsCompatible(IAmmo ammo);
		int ClipSize { get; }
		float ReloadDuration { get; }
		Vector3 RecoilVector { get; }
		IDirectionalObject DirectionalObject { get; set; }
	}
}