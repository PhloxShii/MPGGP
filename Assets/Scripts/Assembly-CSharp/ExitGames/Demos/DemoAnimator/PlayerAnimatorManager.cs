using Photon;
using UnityEngine;

namespace ExitGames.Demos.DemoAnimator
{
	public class PlayerAnimatorManager : Photon.MonoBehaviour
	{
		public float DirectionDampTime = 0.25f;

		private Animator animator;

		private void Start()
		{
			animator = GetComponent<Animator>();
		}

		private void Update()
		{
			if ((base.photonView.isMine || !PhotonNetwork.connected) && (bool)animator)
			{
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run") && Input.GetButtonDown("Fire2"))
				{
					animator.SetTrigger("Jump");
				}
				float axis = Input.GetAxis("Horizontal");
				float num = Input.GetAxis("Vertical");
				if (num < 0f)
				{
					num = 0f;
				}
				animator.SetFloat("Speed", axis * axis + num * num);
				animator.SetFloat("Direction", axis, DirectionDampTime, Time.deltaTime);
			}
		}
	}
}
