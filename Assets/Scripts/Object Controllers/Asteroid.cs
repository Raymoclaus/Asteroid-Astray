using UnityEngine;

public class Asteroid : Entity, IDrillableObject
{

	#region Fields

	[Header("References to objects and components.")]
	[Tooltip("List of available sprites to choose from.")]
	public Sprite[] Shapes;

	[Tooltip("Reference to the sprite renderer of the asteroid.")]
	public SpriteRenderer SprRend;

	[Tooltip("Reference to the shake effect script on the sprite.")]
	public ShakeEffect ShakeFX;

	[Header("Properties of the asteroid.")]
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float SpinSpeedRange;

	[Tooltip("Upper limit for health stat.")]
	public float MaxHealth = 150f;
	
	private float Health;

	#endregion

	public override void Start()
	{
		base.Start();
		//pick a random sprite from given list of sprites
		SprRend.sprite = Shapes[Random.Range(0, Shapes.Length)];
		//picks a random speed to spin at within a given range with chance favoring lower values
		Rb.rotation = Mathf.Pow(Random.Range(0f, 2f), 2f) * SpinSpeedRange - SpinSpeedRange;
		//start health at max value
		Health = MaxHealth;
	}

	private void DestroySelf(bool explode)
	{
		if (explode)
		{
			//particle effect stuff or something
		}

		base.DestroySelf();
	}

	// If health is below zero, this will destroy itself
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
		ShakeFX.SetIntensity(damage / MaxHealth);
		return CheckHealth();
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		ShakeFX.Begin();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill)
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();

			if ((Entity)otherDrill.drillTarget == this)
			{
				otherDrill.StopDrilling();
			}
		}
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		ShakeFX.Stop();
	}

	//If queried, this object will say that it is an asteroid-type Entity
	public override EntityType GetEntityType()
	{
		return EntityType.Asteroid;
	}
}