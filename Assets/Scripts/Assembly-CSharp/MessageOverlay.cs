using UnityEngine;

public class MessageOverlay : MonoBehaviour
{
	public GameObject[] Objects;

	public void Start()
	{
		SetActive(enable: true);
	}

	public void OnJoinedRoom()
	{
		SetActive(enable: false);
	}

	public void OnLeftRoom()
	{
		SetActive(enable: true);
	}

	private void SetActive(bool enable)
	{
		GameObject[] objects = Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(enable);
		}
	}
}
