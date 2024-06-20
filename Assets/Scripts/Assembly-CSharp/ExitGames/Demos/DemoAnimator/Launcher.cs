using System;
using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoAnimator
{
	public class Launcher : PunBehaviour
	{
		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		public GameObject controlPanel;

		[Tooltip("The Ui Text to inform the user about the connection progress")]
		public Text feedbackText;

		[Tooltip("The maximum number of players per room")]
		public byte maxPlayersPerRoom = 4;

		[Tooltip("The UI Loader Anime")]
		public LoaderAnime loaderAnime;

		private bool isConnecting;

		private string _gameVersion = "1";

		private void Awake()
		{
			if (loaderAnime == null)
			{
				Debug.LogError("<Color=Red><b>Missing</b></Color> loaderAnime Reference.", this);
			}
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.automaticallySyncScene = true;
		}

		public void Connect()
		{
			feedbackText.text = "";
			isConnecting = true;
			controlPanel.SetActive(value: false);
			if (loaderAnime != null)
			{
				loaderAnime.StartLoaderAnimation();
			}
			if (PhotonNetwork.connected)
			{
				LogFeedback("Joining Room...");
				PhotonNetwork.JoinRandomRoom();
			}
			else
			{
				LogFeedback("Connecting...");
				PhotonNetwork.ConnectUsingSettings(_gameVersion);
			}
		}

		private void LogFeedback(string message)
		{
			if (!(feedbackText == null))
			{
				Text text = feedbackText;
				text.text = text.text + Environment.NewLine + message;
			}
		}

		public override void OnConnectedToMaster()
		{
			Debug.Log("Region:" + PhotonNetwork.networkingPeer.CloudRegion);
			if (isConnecting)
			{
				LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
				Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");
				PhotonNetwork.JoinRandomRoom();
			}
		}

		public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			LogFeedback("<Color=Red>OnPhotonRandomJoinFailed</Color>: Next -> Create a new Room");
			Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
			PhotonNetwork.CreateRoom(null, new RoomOptions
			{
				MaxPlayers = maxPlayersPerRoom
			}, null);
		}

		public override void OnDisconnectedFromPhoton()
		{
			LogFeedback("<Color=Red>OnDisconnectedFromPhoton</Color>");
			Debug.LogError("DemoAnimator/Launcher:Disconnected");
			loaderAnime.StopLoaderAnimation();
			isConnecting = false;
			controlPanel.SetActive(value: true);
		}

		public override void OnJoinedRoom()
		{
			LogFeedback("<Color=Green>OnJoinedRoom</Color> with " + PhotonNetwork.room.PlayerCount + " Player(s)");
			Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
			if (PhotonNetwork.room.PlayerCount == 1)
			{
				Debug.Log("We load the MAIN GAMEE");
				PhotonNetwork.LoadLevel("Main");
			}
		}
	}
}
