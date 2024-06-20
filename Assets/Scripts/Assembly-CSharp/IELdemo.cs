using UnityEngine;

public class IELdemo : MonoBehaviour
{
	public GUISkin Skin;

	public void OnGUI()
	{
		if (Skin != null)
		{
			GUI.skin = Skin;
		}
		if (PhotonNetwork.isMasterClient)
		{
			GUILayout.Label("Controlling client.\nPing: " + PhotonNetwork.GetPing());
			if (GUILayout.Button("disconnect", GUILayout.ExpandWidth(expand: false)))
			{
				PhotonNetwork.Disconnect();
			}
		}
		else if (PhotonNetwork.isNonMasterClientInRoom)
		{
			GUILayout.Label("Receiving updates.\nPing: " + PhotonNetwork.GetPing());
			if (GUILayout.Button("disconnect", GUILayout.ExpandWidth(expand: false)))
			{
				PhotonNetwork.Disconnect();
			}
		}
		else
		{
			GUILayout.Label("Not in room yet\n" + PhotonNetwork.connectionStateDetailed);
		}
		if (!PhotonNetwork.connected && !PhotonNetwork.connecting && GUILayout.Button("connect", GUILayout.Width(80f)))
		{
			PhotonNetwork.ConnectUsingSettings(null);
		}
	}
}
