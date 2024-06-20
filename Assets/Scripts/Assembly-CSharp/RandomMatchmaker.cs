using Photon;
using UnityEngine;

public class RandomMatchmaker : PunBehaviour
{
	private PhotonView myPhotonView;

	public void Start()
	{
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	public override void OnJoinedLobby()
	{
		Debug.Log("JoinRandom");
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}

	public override void OnJoinedRoom()
	{
		GameObject gameObject = PhotonNetwork.Instantiate("monsterprefab", Vector3.zero, Quaternion.identity, 0);
		gameObject.GetComponent<myThirdPersonController>().isControllable = true;
		myPhotonView = gameObject.GetComponent<PhotonView>();
	}

	public void OnGUI()
	{
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		if (PhotonNetwork.inRoom)
		{
			bool num = GameLogic.playerWhoIsIt == PhotonNetwork.player.ID;
			if (num && GUILayout.Button("Marco!"))
			{
				myPhotonView.RPC("Marco", PhotonTargets.All);
			}
			if (!num && GUILayout.Button("Polo!"))
			{
				myPhotonView.RPC("Polo", PhotonTargets.All);
			}
		}
	}
}
