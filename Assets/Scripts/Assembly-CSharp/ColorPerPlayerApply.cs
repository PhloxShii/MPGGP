using System;
using ExitGames.UtilityScripts;
using Photon;
using UnityEngine;

public class ColorPerPlayerApply : PunBehaviour
{
	private static ColorPerPlayer colorPickerCache;

	private Renderer rendererComponent;

	private bool isInitialized;

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
			instance.OnRoomIndexingChanged = (PlayerRoomIndexing.RoomIndexingChanged)Delegate.Combine(instance.OnRoomIndexingChanged, new PlayerRoomIndexing.RoomIndexingChanged(ApplyColor));
			isInitialized = true;
		}
	}

	private void OnDisable()
	{
		isInitialized = false;
		if (PlayerRoomIndexing.instance != null)
		{
			PlayerRoomIndexing instance = PlayerRoomIndexing.instance;
			instance.OnRoomIndexingChanged = (PlayerRoomIndexing.RoomIndexingChanged)Delegate.Remove(instance.OnRoomIndexingChanged, new PlayerRoomIndexing.RoomIndexingChanged(ApplyColor));
		}
	}

	public void Awake()
	{
		if (colorPickerCache == null)
		{
			colorPickerCache = UnityEngine.Object.FindObjectOfType<ColorPerPlayer>();
		}
		if (colorPickerCache == null)
		{
			base.enabled = false;
		}
		if (base.photonView.isSceneView)
		{
			base.enabled = false;
		}
		rendererComponent = GetComponent<Renderer>();
	}

	public override void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		ApplyColor();
	}

	public void ApplyColor()
	{
		if (base.photonView.owner != null)
		{
			int roomIndex = base.photonView.owner.GetRoomIndex();
			if (roomIndex >= 0 && roomIndex <= colorPickerCache.Colors.Length)
			{
				rendererComponent.material.color = colorPickerCache.Colors[roomIndex];
			}
		}
	}
}
