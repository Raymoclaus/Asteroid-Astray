using UnityEngine;

namespace Object_Controllers
{
	public class Asteroid : Entity, IDrillableObject
	{
		/* Fields */

		#region

		[Header("References to objects and components.")]
		[Tooltip("List of available sprites to choose from.")]
		public Sprite[] Shapes;

		[Tooltip("Reference to the sprite renderer of the asteroid.")]
		public SpriteRenderer SprRend;

		[Header("Properties of the asteroid.")]
		[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
		public float SpinSpeedRange;

		private float _spinSpeed;

		[Tooltip("Health decreases when taking damage. When it reaches 0 the asteroid is destroyed.")]
		public float Health;

		//drillable object related stuff
		private bool _beingDrilled = false;

		#endregion

		public override void Start()
		{
			base.Start();
			//pick a random sprite from given list of sprites
			SprRend.sprite = Shapes[Random.Range(0, Shapes.Length)];
			//picks a random speed to spin at within a given range with chance favoring lower values
			Rb.rotation = Mathf.Pow(Random.Range(0f, 2f), 2f) * SpinSpeedRange - SpinSpeedRange;
		}

		public void Update()
		{
			
		}

		private void DestroySelf(bool explode)
		{
			if (explode)
			{
				//particle effect stuff or something
			}

			base.DestroySelf();
		}

		/// If health is below zero, this will destroy itself
		private bool CheckHealth()
		{
			if (Health > 0f) return false;
			DestroySelf(true);
			return true;
		}

		//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
		public bool TakeDrillDamage(float damage)
		{
			Health -= damage;
			return CheckHealth();
		}

		//If queried, this object will say that it is an asteroid-type Entity
		public override EntityType GetEntityType()
		{
			return EntityType.Asteroid;
		}
	}
}