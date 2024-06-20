using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class CubeExtra : Photon.MonoBehaviour, IPunObservable
{
	[Range(0.9f, 1.1f)]
	public float Factor = 0.98f;

	private Vector3 latestCorrectPos = Vector3.zero;

	private Vector3 movementVector = Vector3.zero;

	private Vector3 errorVector = Vector3.zero;

	private double lastTime;

	public void Awake()
	{
		latestCorrectPos = base.transform.position;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 obj = base.transform.localPosition;
			stream.Serialize(ref obj);
			return;
		}
		Vector3 obj2 = Vector3.zero;
		stream.Serialize(ref obj2);
		double num = info.timestamp - lastTime;
		lastTime = info.timestamp;
		movementVector = (obj2 - latestCorrectPos) / (float)num;
		errorVector = (obj2 - base.transform.localPosition) / (float)num;
		latestCorrectPos = obj2;
	}

	public void Update()
	{
		if (!base.photonView.isMine)
		{
			base.transform.localPosition += (movementVector + errorVector) * Factor * Time.deltaTime;
		}
	}
}
