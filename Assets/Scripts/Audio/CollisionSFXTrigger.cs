using UnityEngine;

public class CollisionSFXTrigger : MonoBehaviour
{
	[SerializeField] private float volumeMultiplier = 1f;
	[SerializeField] private AudioSO collisionSounds;
	protected static AudioManager audioManager;
	protected static AudioManager AudioMngr
	{
		get
		{
			return audioManager ?? (audioManager = FindObjectOfType<AudioManager>());
		}
	}
	[SerializeField] private float ignoreRange = 15f;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	[SerializeField] private float ignoreCollisionStrength = 0.05f;
	private static AudioListener listener;
	private static AudioListener Listener { get { return listener ?? (listener = FindObjectOfType<AudioListener>()); } }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!collisionSounds) return;
		float collisionStrength = collision.relativeVelocity.magnitude * volumeMultiplier;
		if (collisionStrength < ignoreCollisionStrength) return;

		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;
		float distance = Vector2.Distance(contactPoint, Listener.transform.position);
		if (distance > ignoreRange) return;

		AudioMngr?.PlaySFX(collisionSounds.PickRandomClip(), contactPoint, null,
			collisionSounds.PickRandomVolume() * collisionStrength, collisionSounds.PickRandomPitch());
	}
}
