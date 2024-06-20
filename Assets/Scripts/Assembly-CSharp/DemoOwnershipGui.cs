using System;
using ExitGames.UtilityScripts;
using UnityEngine;

public class DemoOwnershipGui : MonoBehaviour
{
	public GUISkin Skin;

	public bool TransferOwnershipOnRequest = true;

	public void OnOwnershipRequest(object[] viewAndPlayer)
	{
		PhotonView photonView = viewAndPlayer[0] as PhotonView;
		PhotonPlayer photonPlayer = viewAndPlayer[1] as PhotonPlayer;
		Debug.Log(string.Concat("OnOwnershipRequest(): Player ", photonPlayer, " requests ownership of: ", photonView, "."));
		if (TransferOwnershipOnRequest)
		{
			photonView.TransferOwnership(photonPlayer.ID);
		}
	}

	public void OnOwnershipTransfered(object[] viewAndPlayers)
	{
		PhotonView photonView = viewAndPlayers[0] as PhotonView;
		PhotonPlayer photonPlayer = viewAndPlayers[1] as PhotonPlayer;
		PhotonPlayer photonPlayer2 = viewAndPlayers[2] as PhotonPlayer;
		Debug.Log(string.Concat("OnOwnershipTransfered for PhotonView", photonView.ToString(), " from ", photonPlayer2, " to ", photonPlayer));
	}

	public void OnGUI()
	{
		GUI.skin = Skin;
		GUILayout.BeginArea(new Rect(Screen.width - 200, 0f, 200f, Screen.height));
		if (GUILayout.Button(TransferOwnershipOnRequest ? "passing objects" : "rejecting to pass"))
		{
			TransferOwnershipOnRequest = !TransferOwnershipOnRequest;
		}
		GUILayout.EndArea();
		if (PhotonNetwork.inRoom)
		{
			int iD = PhotonNetwork.player.ID;
			string arg = (PhotonNetwork.player.IsMasterClient ? "(master) " : "");
			string colorName = GetColorName(PhotonNetwork.player.ID);
			GUILayout.Label($"player {iD}, {colorName} {arg}(you)");
			PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
			foreach (PhotonPlayer photonPlayer in otherPlayers)
			{
				iD = photonPlayer.ID;
				arg = (photonPlayer.IsMasterClient ? "(master)" : "");
				colorName = GetColorName(photonPlayer.ID);
				GUILayout.Label($"player {iD}, {colorName} {arg}");
			}
			if (PhotonNetwork.inRoom && PhotonNetwork.otherPlayers.Length == 0)
			{
				GUILayout.Label("Join more clients to switch object-control.");
			}
		}
		else
		{
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		}
	}

	private string GetColorName(int playerId)
	{
		switch (Array.IndexOf(PlayerRoomIndexing.instance.PlayerIds, playerId))
		{
		case 0:
			return "red";
		case 1:
			return "blue";
		case 2:
			return "yellow";
		case 3:
			return "green";
		default:
			return string.Empty;
		}
	}
}
