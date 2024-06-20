using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoAnimator
{
	public class PlayerUI : MonoBehaviour
	{
		[Tooltip("Pixel offset from the player target")]
		public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);

		[Tooltip("UI Text to display Player's Name")]
		public Text PlayerNameText;

		[Tooltip("UI Slider to display Player's Health")]
		public Slider PlayerHealthSlider;

		private PlayerManager _target;

		private float _characterControllerHeight;

		private Transform _targetTransform;

		private Renderer _targetRenderer;

		private Vector3 _targetPosition;

		private void Awake()
		{
			GetComponent<Transform>().SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
		}

		private void Update()
		{
			if (_target == null)
			{
				Object.Destroy(base.gameObject);
			}
			else if (PlayerHealthSlider != null)
			{
				PlayerHealthSlider.value = _target.Health;
			}
		}

		private void LateUpdate()
		{
			if (_targetRenderer != null)
			{
				base.gameObject.SetActive(_targetRenderer.isVisible);
			}
			if (_targetTransform != null)
			{
				_targetPosition = _targetTransform.position;
				_targetPosition.y += _characterControllerHeight;
				base.transform.position = Camera.main.WorldToScreenPoint(_targetPosition) + ScreenOffset;
			}
		}

		public void SetTarget(PlayerManager target)
		{
			if (target == null)
			{
				Debug.LogError("<Color=Red><b>Missing</b></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
				return;
			}
			_target = target;
			_targetTransform = _target.GetComponent<Transform>();
			_targetRenderer = _target.GetComponent<Renderer>();
			CharacterController component = _target.GetComponent<CharacterController>();
			if (component != null)
			{
				_characterControllerHeight = component.height;
			}
			if (PlayerNameText != null)
			{
				PlayerNameText.text = _target.photonView.owner.NickName;
			}
		}
	}
}
