using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResourceDrop : MonoBehaviour
{
	[HideInInspector]
	public Transform follow;
	private Vector2 velocity;
	private Vector2 startVelocity;
	public float startSpeed = 0.1f;
	public float speedGain = 0.001f;
	private float speedIncrement;
	public float delay = 0.5f;
	private float spawnTime;
	public ParticleSystem ps;

	private void Start()
	{
		follow = Shuttle.singleton.transform;
		ps = ps ?? transform.GetChild(0).GetComponent<ParticleSystem>();

		//pick a random direction to move in
		startVelocity = new Vector2(Random.value * 2f - 1f, Random.value * 2f - 1f);
		startVelocity.Normalize();
		startVelocity *= startSpeed;

		spawnTime = Time.time;
	}

	private void Update()
	{
		float aliveTime = Time.time - spawnTime;

		if (aliveTime < delay)
		{
			velocity = Vector2.Lerp(startVelocity, Vector2.zero, aliveTime / delay);
		}
		else
		{
			//gain speed towards the follow target
			Vector2 direction = follow.position - transform.position;
			direction.Normalize();
			speedIncrement += speedGain;
			direction *= speedIncrement;
			velocity = direction;
		}

		//set the position
		transform.position += (Vector3)velocity;

		//check if close enough to collect
		if (Vector2.Distance(transform.position, follow.position) < velocity.magnitude && aliveTime > delay)
		{
			//send messsage to follow target to collect resource

			//destroy self
			ps.transform.parent = null;
			ParticleSystem.MainModule main = ps.main;
			main.loop = false;
			Shuttle.singleton.CollectResources(this);
			Destroy(gameObject);
			return;
		}
	}
}