using UnityEngine;

namespace ParticleSystemControllers
{
	public class PickupTrailController : MonoBehaviour
	{
		[SerializeField] private Transform targetToFollow;
		[SerializeField] private ParticleSystem ps;

		private void Update()
		{
			if (targetToFollow == null) return;

			transform.position = targetToFollow.position;
		}

		public void SetTarget(Transform t) => targetToFollow = t;

		public void SetLooping(bool looping)
		{
			ParticleSystem.MainModule main = ps.main;
			main.loop = looping;
		}

		public void SetColor(Color col)
		{
			ParticleSystem.MainModule main = ps.main;
			main.startColor = col;
		}
	}
}