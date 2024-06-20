using System;
using ExitGames.UtilityScripts;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MaterialPerOwner : Photon.MonoBehaviour
{
	private int assignedColorForUserId;

	private Renderer m_Renderer;

	private void Start()
	{
		m_Renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		if (base.photonView.ownerId != assignedColorForUserId)
		{
			int num = Array.IndexOf(PlayerRoomIndexing.instance.PlayerIds, base.photonView.ownerId);
			try
			{
				m_Renderer.material.color = UnityEngine.Object.FindObjectOfType<ColorPerPlayer>().Colors[num];
				assignedColorForUserId = base.photonView.ownerId;
			}
			catch (Exception)
			{
			}
		}
	}
}
