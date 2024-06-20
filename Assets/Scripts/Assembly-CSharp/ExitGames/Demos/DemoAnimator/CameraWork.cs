using UnityEngine;

namespace ExitGames.Demos.DemoAnimator
{
	public class CameraWork : MonoBehaviour
	{
		[Tooltip("The distance in the local x-z plane to the target")]
		public float distance = 7f;

		[Tooltip("The height we want the camera to be above the target")]
		public float height = 3f;

		[Tooltip("The Smooth time lag for the height of the camera.")]
		public float heightSmoothLag = 0.3f;

		[Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
		public Vector3 centerOffset = Vector3.zero;

		[Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
		public bool followOnStart;

		private Transform cameraTransform;

		private bool isFollowing;

		private float heightVelocity;

		private float targetHeight = 100000f;

		private void Start()
		{
			if (followOnStart)
			{
				OnStartFollowing();
			}
		}

		private void LateUpdate()
		{
			if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}
			if (isFollowing)
			{
				Apply();
			}
		}

		public void OnStartFollowing()
		{
			cameraTransform = Camera.main.transform;
			isFollowing = true;
			Cut();
		}

		private void Apply()
		{
			Vector3 vector = base.transform.position + centerOffset;
			float y = base.transform.eulerAngles.y;
			float y2 = cameraTransform.eulerAngles.y;
			y2 = y;
			targetHeight = vector.y + height;
			float y3 = cameraTransform.position.y;
			y3 = Mathf.SmoothDamp(y3, targetHeight, ref heightVelocity, heightSmoothLag);
			Quaternion quaternion = Quaternion.Euler(0f, y2, 0f);
			cameraTransform.position = vector;
			cameraTransform.position += quaternion * Vector3.back * distance;
			cameraTransform.position = new Vector3(cameraTransform.position.x, y3, cameraTransform.position.z);
			SetUpRotation(vector);
		}

		private void Cut()
		{
			float num = heightSmoothLag;
			heightSmoothLag = 0.001f;
			Apply();
			heightSmoothLag = num;
		}

		private void SetUpRotation(Vector3 centerPos)
		{
			Vector3 position = cameraTransform.position;
			Vector3 vector = centerPos - position;
			Quaternion quaternion = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z));
			Vector3 forward = Vector3.forward * distance + Vector3.down * height;
			cameraTransform.rotation = quaternion * Quaternion.LookRotation(forward);
		}
	}
}
