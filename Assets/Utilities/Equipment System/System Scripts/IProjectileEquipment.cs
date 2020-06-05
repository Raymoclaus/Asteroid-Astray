using AttackData;
using UnityEngine;

namespace EquipmentSystem
{
	public interface IProjectileEquipment : ITriggerableEquipment
	{
		new IAmmo GetAttack { get; }
		bool IsCompatible(IAmmo ammo);
		int ClipSize { get; }
		float ReloadDuration { get; }
		Vector3 RecoilVector { get; }
	}
}