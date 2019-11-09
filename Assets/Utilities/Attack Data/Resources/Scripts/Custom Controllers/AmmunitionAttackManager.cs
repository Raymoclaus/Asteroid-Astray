using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(IsProjectileComponent), typeof(DamageComponent), typeof(VelocityComponent))]
	[RequireComponent(typeof(DirectionComponent))]
	public class AmmunitionAttackManager : AttackManager, IAmmo
	{
		[SerializeField] private float baseDamage;
		protected Vector3 InitialVelocity { get; set; }
		protected Vector3 InitialWeaponDirection { get; set; }
		protected Vector3 InitialWeaponPosition { get; set; }

		private VelocityComponent velocityComp;

		private VelocityComponent VelocityComp
			=> velocityComp != null
				? velocityComp
				: (velocityComp = GetAttackComponent<VelocityComponent>());

		private DirectionComponent directionComp;
		private DirectionComponent DirectionComp
			=> directionComp != null
				? directionComp
				: (directionComp = GetAttackComponent<DirectionComponent>());

		private DamageComponent damageComp;
		private DamageComponent DamageComp
			=> damageComp != null
				? damageComp
				: (damageComp = GetAttackComponent<DamageComponent>());

		protected override void Awake()
		{
			base.Awake();
			Damage = baseDamage;
		}

		public void MultiplyDamage(float multiplier) => DamageComp.damage *= multiplier;

		public void SetIntialVelocity(Vector3 velocity)
		{
			InitialVelocity = velocity;
			InitialSpeed = velocity.magnitude;
			Velocity = velocity;
		}

		protected float InitialSpeed { get; set; }

		public void SetInitialWeaponDirection(Vector3 direction)
			=> InitialWeaponDirection = direction;

		public void SetInitialWeaponPosition(Vector3 position)
			=> InitialWeaponPosition = position;

		protected float CurrentSpeed
		{
			get => Velocity.magnitude;
			set => Velocity = Direction * value;
		}

		protected Vector3 Direction
		{
			get => Velocity.normalized;
			set => Velocity = value.normalized * CurrentSpeed;
		}

		protected Vector3 Velocity
		{
			get => VelocityComp.velocity;
			set
			{
				VelocityComp.velocity = value;
				DirectionComp.direction = value.normalized;
			}
		}

		protected float Damage
		{
			get => DamageComp.damage;
			set => DamageComp.damage = value;
		}
	}
}
