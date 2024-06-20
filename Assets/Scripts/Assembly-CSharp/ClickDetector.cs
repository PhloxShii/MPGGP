using System;
using UnityEngine;

public class ClickDetector : MonoBehaviour
{
	public void Update()
	{
		if (PhotonNetwork.player.ID == GameLogic.playerWhoIsIt && Input.GetButton("Fire1"))
		{
			GameObject gameObject = RaycastObject(Input.mousePosition);
			if (gameObject != null && gameObject != base.gameObject && gameObject.name.Equals("monsterprefab(Clone)", StringComparison.OrdinalIgnoreCase))
			{
				GameLogic.TagPlayer(gameObject.transform.root.GetComponent<PhotonView>().owner.ID);
			}
		}
	}

	private GameObject RaycastObject(Vector2 screenPos)
	{
		if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out var hitInfo, 200f))
		{
			return hitInfo.collider.gameObject;
		}
		return null;
	}
}
