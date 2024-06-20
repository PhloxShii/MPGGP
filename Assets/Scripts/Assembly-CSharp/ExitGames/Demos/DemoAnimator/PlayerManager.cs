using Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ExitGames.Demos.DemoAnimator
{
	public class PlayerManager : PunBehaviour, IPunObservable
	{
		[Tooltip("The Player's UI GameObject Prefab")]
		public GameObject PlayerUiPrefab;

		[Tooltip("The Beams GameObject to control")]
		public GameObject Beams;

		[Tooltip("The current Health of our player")]
		public float Health = 1f;

		[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
		public static GameObject LocalPlayerInstance;

		private bool IsFiring;

		public void Awake()
		{
			if (Beams == null)
			{
				Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
			}
			else
			{
				Beams.SetActive(value: false);
			}
			if (base.photonView.isMine)
			{
				LocalPlayerInstance = base.gameObject;
			}
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public void Start()
		{
			CameraWork component = base.gameObject.GetComponent<CameraWork>();
			if (component != null)
			{
				if (base.photonView.isMine)
				{
					component.OnStartFollowing();
				}
			}
			else
			{
				Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
			}
			if (PlayerUiPrefab != null)
			{
				Object.Instantiate(PlayerUiPrefab).SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
			}
			else
			{
				Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		public void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void Update()
		{
			if (base.photonView.isMine)
			{
				ProcessInputs();
				if (Health <= 0f)
				{
					GameManager.Instance.LeaveRoom();
				}
			}
			if (Beams != null && IsFiring != Beams.GetActive())
			{
				Beams.SetActive(IsFiring);
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			if (base.photonView.isMine && other.name.Contains("Beam"))
			{
				Health -= 0.1f;
			}
		}

		public void OnTriggerStay(Collider other)
		{
			if (base.photonView.isMine && other.name.Contains("Beam"))
			{
				Health -= 0.1f * Time.deltaTime;
			}
		}

		private void CalledOnLevelWasLoaded(int level)
		{
			if (!Physics.Raycast(base.transform.position, -Vector3.up, 5f))
			{
				base.transform.position = new Vector3(0f, 5f, 0f);
			}
			Object.Instantiate(PlayerUiPrefab).SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode)
		{
			CalledOnLevelWasLoaded(scene.buildIndex);
		}

		private void ProcessInputs()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				EventSystem.current.IsPointerOverGameObject();
				if (!IsFiring)
				{
					IsFiring = true;
				}
			}
			if (Input.GetButtonUp("Fire1") && IsFiring)
			{
				IsFiring = false;
			}
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				stream.SendNext(IsFiring);
				stream.SendNext(Health);
			}
			else
			{
				IsFiring = (bool)stream.ReceiveNext();
				Health = (float)stream.ReceiveNext();
			}
		}
	}
}
