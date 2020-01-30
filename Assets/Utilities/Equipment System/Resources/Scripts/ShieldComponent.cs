using UnityEngine;

namespace EquipmentSystem
{
	public class ShieldComponent : MonoBehaviour
	{
		private IShieldMaterial shieldMat;

		private IShieldMaterial ShieldMat => shieldMat != null
			? shieldMat
			: (shieldMat = GetComponent<IShieldMaterial>());

		public void TakeHit(Vector3 direction)
		{
			ShieldMat.TakeHit(direction);
		}
	} 
}
