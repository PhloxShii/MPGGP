using Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorkerInGame : Photon.MonoBehaviour
{
	public Transform playerPrefab;

	public void Awake()
	{
		if (!PhotonNetwork.connected)
		{
			SceneManager.LoadScene("LoginButBetter");
		}
		else
		{
			PhotonNetwork.Instantiate(playerPrefab.name, base.transform.position, Quaternion.identity, 0);
		}
	}

	public void OnGUI()
	{
		if (GUILayout.Button("Return to Lobby"))
		{
			PhotonNetwork.LeaveRoom();
		}
	}

	public void OnMasterClientSwitched(PhotonPlayer player)
	{
		Debug.Log("OnMasterClientSwitched: " + player);
		InRoomChat component = GetComponent<InRoomChat>();
		if (component != null)
		{
			string newLine = ((!player.IsLocal) ? (player.NickName + " is Master Client now.") : "You are Master Client now.");
			component.AddLine(newLine);
		}
	}

	public void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom (local)");
		SceneManager.LoadScene("LoginButBetter");
	}

	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton");
		SceneManager.LoadScene("LoginButBetter");
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		Debug.Log("OnPhotonInstantiate " + info.sender);
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("OnPlayerDisconneced: " + player);
	}

	public void OnFailedToConnectToPhoton()
	{
		Debug.Log("OnFailedToConnectToPhoton");
		SceneManager.LoadScene("LoginButBetter");
	}
}
