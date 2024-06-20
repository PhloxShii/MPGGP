using Photon;
using UnityEngine;

public class OnDoubleclickDestroy : Photon.MonoBehaviour
{
	private float timeOfLastClick;

	private const float ClickDeltaForDoubleclick = 0.2f;

	private void OnClick()
	{
		if (base.photonView.isMine)
		{
			if (Time.time - timeOfLastClick < 0.2f)
			{
				PhotonNetwork.Destroy(base.gameObject);
			}
			else
			{
				timeOfLastClick = Time.time;
			}
		}
	}
}
