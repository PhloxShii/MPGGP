using System.Collections;
using Photon;
using UnityEngine;

public class OnClickFlashRpc : PunBehaviour
{
	private Material originalMaterial;

	private Color originalColor;

	private bool isFlashing;

	private void OnClick()
	{
		base.photonView.RPC("Flash", PhotonTargets.All);
	}

	[PunRPC]
	private IEnumerator Flash()
	{
		if (isFlashing)
		{
			yield break;
		}
		isFlashing = true;
		originalMaterial = GetComponent<Renderer>().material;
		if (!originalMaterial.HasProperty("_Emission"))
		{
			Debug.LogWarning("Doesnt have emission, can't flash " + base.gameObject);
			yield break;
		}
		originalColor = originalMaterial.GetColor("_Emission");
		originalMaterial.SetColor("_Emission", Color.white);
		for (float f = 0f; f <= 1f; f += 0.08f)
		{
			Color value = Color.Lerp(Color.white, originalColor, f);
			originalMaterial.SetColor("_Emission", value);
			yield return null;
		}
		originalMaterial.SetColor("_Emission", originalColor);
		isFlashing = false;
	}
}
