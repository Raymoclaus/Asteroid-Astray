using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidCtrl : DrillableObject
{
	/* Fields */
	#region
	[Header("References to objects and components.")]
	[Tooltip("List of available sprites to choose from.")]
	public Sprite[] shapes;
	[Tooltip("Reference to the sprite renderer of the asteroid.")]
	public SpriteRenderer sprRend;
	[Tooltip("Reference to the CircleCollider2D of the asteroid.")]
	public CircleCollider2D col;
	//reference to the asteroid generator (to be given by the generator upon creation)
	private AsteroidGenerator asterGen;

	//details including what chunk this asteroid is in and its ID within that chunk
	[HideInInspector]
	public ChunkCoordinates chunkRef;
	[HideInInspector]
	public int chunkPart;
	[HideInInspector]
	public Vector2 Position {get {return (Vector2)transform.position; }}

	[Header("Properties of the asteroid.")]
	[Tooltip("Picks a random value between given value and negative given value to determine its rotation speed")]
	public float spinSpeedRange;
	private float spinSpeed;
	[Tooltip("Health decreases when taking damage such as via drilling. When it reaches 0 the asteroid is destroyed.")]
	public float health;
	private bool isBeingDrilled;
	#endregion

	void Awake()
	{
		//pick a random sprite from given list of sprites
		sprRend.sprite = shapes[Random.Range(0, shapes.Length)];
		spinSpeed = Mathf.Pow(Random.Range(-1f, 1f), 3f) * spinSpeedRange;
	}

	void Update()
	{
		transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("DeadZone"))
		{
			DestroySelf(false);
		}
	}

	//initialises variables given by the generator
	public void ChunkRefInit(ChunkCoordinates chCoord, int part, AsteroidGenerator astGen)
	{
		chunkRef = chCoord;
		chunkPart = part;
		asterGen = astGen;
		//if colliding with another asteroid then it moves to a new position
		MoveToFreePosition(chunkPart);
	}

	//if colliding with other asteroids, it will try to find a new empty position
	private void MoveToFreePosition(int part)
	{
		int freezeCheck = 0;
		//pick a direction
		int angle = Random.Range(0, 360);
		Vector3 move = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle), 0f);
		//move in that direction until no longer colliding
		while (CheckCollision())
		{
			transform.position += move * col.radius;

			freezeCheck++;
			if (freezeCheck >= Cnsts.FREEZE_LIMIT)
			{
				break;
			}
		}
	}

	//checks if colliding with "Solid" (such as other asteroids except itself)
	private bool CheckCollision()
	{
		foreach (Collider2D other in 
			Physics2D.OverlapCircleAll(col.bounds.center, col.radius * 2f, 1 << LayerMask.NameToLayer("Solid")))
		{
			if (other.gameObject != gameObject)
			{
				return true;
			}
		}
		return false;
	}

	//tells the generator that it will destroy itself, then does so.
	//Explode boolean determines whether particle effects should be made
	public void DestroySelf(bool explode)
	{
		asterGen.DestroyAsteroid(chunkRef, chunkPart);
		if (explode)
		{
			//particle effect stuff or something
		}
		Destroy(gameObject, 0f);
	}

	private bool CheckHealth()
	{
		if (health <= 0f)
		{
			DestroySelf(true);
			return true;
		}
		return false;
	}

	//take the damage and if health drops to 0 then signal that this asteroid will be destroyed
	public override bool TakeDrillDamage(float damage)
	{
		health -= damage;
		return CheckHealth();
	}
}
