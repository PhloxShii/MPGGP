using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ExitGames.Demos
{
	public class ToDemoHubButton : MonoBehaviour
	{
		private static ToDemoHubButton instance;

		private CanvasGroup _canvasGroup;

		public static ToDemoHubButton Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType(typeof(ToDemoHubButton)) as ToDemoHubButton;
				}
				return instance;
			}
		}

		public void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void Start()
		{
			Object.DontDestroyOnLoad(base.gameObject);
			_canvasGroup = GetComponent<CanvasGroup>();
			SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode loadingMode)
			{
				CalledOnLevelWasLoaded(scene.buildIndex);
			};
		}

		private void CalledOnLevelWasLoaded(int level)
		{
			Debug.Log("CalledOnLevelWasLoaded");
			if (EventSystem.current == null)
			{
				Debug.LogError("no eventSystem");
			}
		}

		public void Update()
		{
			bool num = SceneManager.GetActiveScene().buildIndex == 0;
			if (num && _canvasGroup.alpha != 0f)
			{
				_canvasGroup.alpha = 0f;
				_canvasGroup.interactable = false;
			}
			if (!num && _canvasGroup.alpha != 1f)
			{
				_canvasGroup.alpha = 1f;
				_canvasGroup.interactable = true;
			}
		}

		public void BackToHub()
		{
			PhotonNetwork.Disconnect();
			SceneManager.LoadScene(0);
		}
	}
}
