using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DemoBoxesGui : MonoBehaviour
{
	public bool HideUI;

	public Text GuiTextForTips;

	private int tipsIndex;

	private readonly string[] tips = new string[11]
	{
		"Click planes to instantiate boxes.", "Click a box to send an RPC. This will flash the box.", "Double click a box to destroy it. If it's yours.", "Boxes send ~10 updates per second when moving.", "Movement is not smoothed at all. It shows the updates 1:1.", "The script ColorPerPlayer assigns a color per player.", "When players leave, their boxes get destroyed. That's called clean up.", "Scene Objects are not cleaned up. The Master Client can Instantiate them.", "Scene Objects are not colored. They are controlled by the Master Client.", "The elevated planes instantiate Scene Objects. Those don't get cleaned up.",
		"Are you still reading?"
	};

	private const float TimePerTip = 3f;

	private float timeSinceLastTip;

	private const float FadeSpeedForTip = 0.05f;

	private void Update()
	{
		if (!(GuiTextForTips == null))
		{
			timeSinceLastTip += Time.deltaTime;
			if (timeSinceLastTip > 3f)
			{
				timeSinceLastTip = 0f;
				StartCoroutine("SwapTip");
			}
		}
	}

	public IEnumerator SwapTip()
	{
		float alpha = 1f;
		while (alpha > 0f)
		{
			alpha -= 0.05f;
			timeSinceLastTip = 0f;
			GuiTextForTips.color = new Color(GuiTextForTips.color.r, GuiTextForTips.color.r, GuiTextForTips.color.r, alpha);
			yield return null;
		}
		tipsIndex = (tipsIndex + 1) % tips.Length;
		GuiTextForTips.text = tips[tipsIndex];
		while (alpha < 1f)
		{
			alpha += 0.05f;
			timeSinceLastTip = 0f;
			GuiTextForTips.color = new Color(GuiTextForTips.color.r, GuiTextForTips.color.r, GuiTextForTips.color.r, alpha);
			yield return null;
		}
	}

	private void OnGUI()
	{
		if (HideUI)
		{
			return;
		}
		GUILayout.BeginArea(new Rect(0f, 0f, 300f, Screen.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		if (!PhotonNetwork.connected)
		{
			if (GUILayout.Button("Connect", GUILayout.Width(100f)))
			{
				PhotonNetwork.ConnectUsingSettings(null);
			}
		}
		else if (GUILayout.Button("Disconnect", GUILayout.Width(100f)))
		{
			PhotonNetwork.Disconnect();
		}
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
