using UnityEngine;

namespace ExitGames.Demos.DemoAnimator
{
	public class LoaderAnime : MonoBehaviour
	{
		[Tooltip("Angular Speed in degrees per seconds")]
		public float speed = 180f;

		[Tooltip("Radius os the loader")]
		public float radius = 1f;

		public GameObject particles;

		private Vector3 _offset;

		private Transform _transform;

		private Transform _particleTransform;

		private bool _isAnimating;

		private void Awake()
		{
			_particleTransform = particles.GetComponent<Transform>();
			_transform = GetComponent<Transform>();
		}

		private void Update()
		{
			if (_isAnimating)
			{
				_transform.Rotate(0f, 0f, speed * Time.deltaTime);
				_particleTransform.localPosition = Vector3.MoveTowards(_particleTransform.localPosition, _offset, 0.5f * Time.deltaTime);
			}
		}

		public void StartLoaderAnimation()
		{
			_isAnimating = true;
			_offset = new Vector3(radius, 0f, 0f);
			particles.SetActive(value: true);
		}

		public void StopLoaderAnimation()
		{
			particles.SetActive(value: false);
		}
	}
}
