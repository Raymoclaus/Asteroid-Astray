using UnityEngine;

public class CollisionSFXTrigger : MonoBehaviour
{
	[SerializeField]
	private float volumeMultiplier = 1f;
	[SerializeField]
	private AudioSO collisionSounds;
	protected static AudioManager audioManager;
	[SerializeField]
	private float ignoreRange = 15f;
	[SerializeField]
	private CameraCtrlTracker camTrackerSO;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!collisionSounds) return;

		ContactPoint2D[] contacts = new ContactPoint2D[1];
		collision.GetContacts(contacts);
		Vector2 contactPoint = contacts[0].point;
		if (Vector2.Distance(contactPoint, camTrackerSO.position) > ignoreRange) return;

		float collisionStrength = collision.relativeVelocity.magnitude * volumeMultiplier;
		if (collisionStrength < 0.05f) return;

		audioManager = audioManager ?? FindObjectOfType<AudioManager>();
		if (audioManager)
		{
			audioManager.PlaySFX(collisionSounds.PickRandomClip(), contactPoint, null,
			collisionSounds.PickRandomVolume() * collisionStrength, collisionSounds.PickRandomPitch());
		}
	}
}
