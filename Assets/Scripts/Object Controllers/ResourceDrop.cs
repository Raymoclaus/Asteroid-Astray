using InventorySystem;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResourceDrop : MonoBehaviour
{
	[HideInInspector]
	public IInventoryHolder follow;
	private Vector2 velocity;
	private Vector2 startVelocity;
	public float startSpeed = 0.1f, speedDecay = 0.01f;
	public float speedGain = 0.001f;
	private float speedIncrement;
	public float delay = 0.5f;
	private float spawnTime;
	public ParticleSystem ps;
	public SpriteRenderer rend;
	private ItemObject type;

	private void Start()
	{
		ps = ps ?? transform.GetChild(0).GetComponent<ParticleSystem>();

		//pick a random direction to move in
		startVelocity = new Vector2(Random.value * 2f - 1f, Random.value * 2f - 1f);
		startVelocity.Normalize();
		startVelocity *= startSpeed;
		velocity = startVelocity;

		spawnTime = TimeController.TimeSinceOpen;
	}

	private void Update()
	{
		if (follow == null) return;

		float aliveTime = TimeController.TimeSinceOpen - spawnTime;

		if (aliveTime < delay || follow == null)
		{
			velocity = Vector2.Lerp(velocity, velocity * speedDecay, Time.timeScale);
		}
		else
		{
			//gain speed towards the follow target
			Vector2 direction = follow.DefaultInventory.transform.position - transform.position;
			direction.Normalize();
			speedIncrement += speedGain * Time.deltaTime * 60f;
			direction *= speedIncrement * Time.deltaTime * 60f;
			velocity = direction;
		}

		//set the position
		float magnitude = velocity.magnitude * Time.deltaTime * 60f;
		transform.position += (Vector3)velocity * Time.deltaTime * 60f;

		//check if close enough to collect
		if (Vector2.Distance(transform.position, follow.DefaultInventory.transform.position) < magnitude
			&& aliveTime > delay)
		{
			//send messsage to follow target to collect resource

			//destroy self
			ps.transform.parent = transform.parent;
			ParticleSystem.MainModule main = ps.main;
			main.loop = false;
			follow.GiveItem(type);
			Destroy(gameObject);
			return;
		}
	}

	public void Create(IInventoryHolder target, Vector2 pos, ItemObject type)
	{
		follow = target;
		transform.position = pos;
		transform.parent = ParticleGenerator.holder;
		this.type = type;
	}
}