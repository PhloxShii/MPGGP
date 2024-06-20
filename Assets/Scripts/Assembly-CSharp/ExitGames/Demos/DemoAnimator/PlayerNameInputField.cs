using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoAnimator
{
	[RequireComponent(typeof(InputField))]
	public class PlayerNameInputField : MonoBehaviour
	{
		private static string playerNamePrefKey = "PlayerName";

		private void Start()
		{
			string playerName = "";
			InputField component = GetComponent<InputField>();
			if (component != null && PlayerPrefs.HasKey(playerNamePrefKey))
			{
				playerName = (component.text = PlayerPrefs.GetString(playerNamePrefKey));
			}
			PhotonNetwork.playerName = playerName;
		}

		public void SetPlayerName(string value)
		{
			PhotonNetwork.playerName = value + " ";
			PlayerPrefs.SetString(playerNamePrefKey, value);
		}
	}
}
