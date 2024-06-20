using System;
using ExitGames.UtilityScripts;
using Photon;
using UnityEngine;

public class ColorPerPlayer : PunBehaviour
{
	public Color[] Colors = new Color[4]
	{
		Color.red,
		Color.blue,
		Color.yellow,
		Color.green
	};

	public const string ColorProp = "pc";

	public bool ShowColorLabel;

	public Rect ColorLabelArea = new Rect(0f, 50f, 100f, 200f);

	public Texture2D img;

	public Color MyColor = Color.grey;

	private bool isInitialized;

	public bool ColorPicked { get; set; }

	private void OnEnable()
	{
		if (!isInitialized)
		{
			Init();
		}
	}

	private void Start()
	{
		if (!isInitialized)
		{
			Init();
		}
	}

	private void Init()
	{
		if (!isInitialized && PlayerRoomIndexing.instance != null)
		{
			PlayerRoomIndexing instance = PlayerRoomIndexing.instance;
			instance.OnRoomIndexingChanged = (PlayerRoomIndexing.RoomIndexingChanged)Delegate.Combine(instance.OnRoomIndexingChanged, new PlayerRoomIndexing.RoomIndexingChanged(Refresh));
			isInitialized = true;
		}
	}

	private void OnDisable()
	{
		PlayerRoomIndexing instance = PlayerRoomIndexing.instance;
		instance.OnRoomIndexingChanged = (PlayerRoomIndexing.RoomIndexingChanged)Delegate.Remove(instance.OnRoomIndexingChanged, new PlayerRoomIndexing.RoomIndexingChanged(Refresh));
	}

	private void Refresh()
	{
		int roomIndex = PhotonNetwork.player.GetRoomIndex();
		if (roomIndex == -1)
		{
			Reset();
			return;
		}
		MyColor = Colors[roomIndex];
		ColorPicked = true;
	}

	public override void OnJoinedRoom()
	{
		if (!isInitialized)
		{
			Init();
		}
	}

	public override void OnLeftRoom()
	{
		Reset();
	}

	public void Reset()
	{
		MyColor = Color.grey;
		ColorPicked = false;
	}

	private void OnGUI()
	{
		if (ColorPicked && ShowColorLabel)
		{
			GUILayout.BeginArea(ColorLabelArea);
			GUILayout.BeginHorizontal();
			Color color = GUI.color;
			GUI.color = MyColor;
			GUILayout.Label(img);
			GUI.color = color;
			GUILayout.Label(PhotonNetwork.isMasterClient ? "is your color\nyou are the Master Client" : "is your color");
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
