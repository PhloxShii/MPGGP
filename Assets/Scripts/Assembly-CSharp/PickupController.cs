using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PickupController : MonoBehaviour, IPunObservable
{
	public AnimationClip idleAnimation;

	public AnimationClip walkAnimation;

	public AnimationClip runAnimation;

	public AnimationClip jumpPoseAnimation;

	public float walkMaxAnimationSpeed = 0.75f;

	public float trotMaxAnimationSpeed = 1f;

	public float runMaxAnimationSpeed = 1f;

	public float jumpAnimationSpeed = 1.15f;

	public float landAnimationSpeed = 1f;

	private Animation _animation;

	public PickupCharacterState _characterState;

	public float walkSpeed = 2f;

	public float trotSpeed = 4f;

	public float runSpeed = 6f;

	public float inAirControlAcceleration = 3f;

	public float jumpHeight = 0.5f;

	public float gravity = 20f;

	public float speedSmoothing = 10f;

	public float rotateSpeed = 500f;

	public float trotAfterSeconds = 3f;

	public bool canJump;

	private float jumpRepeatTime = 0.05f;

	private float jumpTimeout = 0.15f;

	private float groundedTimeout = 0.25f;

	private float lockCameraTimer;

	private Vector3 moveDirection = Vector3.zero;

	private float verticalSpeed;

	private float moveSpeed;

	private CollisionFlags collisionFlags;

	private bool jumping;

	private bool jumpingReachedApex;

	private bool movingBack;

	private bool isMoving;

	private float walkTimeStart;

	private float lastJumpButtonTime = -10f;

	private float lastJumpTime = -1f;

	private Vector3 inAirVelocity = Vector3.zero;

	private float lastGroundedTime;

	private Vector3 velocity = Vector3.zero;

	private Vector3 lastPos;

	private Vector3 remotePosition;

	public bool isControllable;

	public bool DoRotate = true;

	public float RemoteSmoothing = 5f;

	public bool AssignAsTagObject = true;

	private void Awake()
	{
		PhotonView component = base.gameObject.GetComponent<PhotonView>();
		if (component != null)
		{
			isControllable = component.isMine;
			if (AssignAsTagObject)
			{
				component.owner.TagObject = base.gameObject;
			}
			if (!DoRotate && component.ObservedComponents != null)
			{
				for (int i = 0; i < component.ObservedComponents.Count; i++)
				{
					if (component.ObservedComponents[i] is Transform)
					{
						component.onSerializeTransformOption = OnSerializeTransform.OnlyPosition;
						break;
					}
				}
			}
		}
		moveDirection = base.transform.TransformDirection(Vector3.forward);
		_animation = GetComponent<Animation>();
		if (!_animation)
		{
			Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		}
		if (!idleAnimation)
		{
			_animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		if (!walkAnimation)
		{
			_animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		if (!runAnimation)
		{
			_animation = null;
			Debug.Log("No run animation found. Turning off animations.");
		}
		if (!jumpPoseAnimation && canJump)
		{
			_animation = null;
			Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
		}
	}

	private void Update()
	{
		if (isControllable)
		{
			if (Input.GetButtonDown("Jump"))
			{
				lastJumpButtonTime = Time.time;
			}
			UpdateSmoothedMovementDirection();
			ApplyGravity();
			ApplyJumping();
			Vector3 motion = moveDirection * moveSpeed + new Vector3(0f, verticalSpeed, 0f) + inAirVelocity;
			motion *= Time.deltaTime;
			CharacterController component = GetComponent<CharacterController>();
			collisionFlags = component.Move(motion);
		}
		if (remotePosition != Vector3.zero)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, remotePosition, Time.deltaTime * RemoteSmoothing);
		}
		velocity = (base.transform.position - lastPos) * 25f;
		if ((bool)_animation)
		{
			if (_characterState == PickupCharacterState.Jumping)
			{
				if (!jumpingReachedApex)
				{
					_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
					_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(jumpPoseAnimation.name);
				}
				else
				{
					_animation[jumpPoseAnimation.name].speed = 0f - landAnimationSpeed;
					_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(jumpPoseAnimation.name);
				}
			}
			else
			{
				if (_characterState == PickupCharacterState.Idle)
				{
					_animation.CrossFade(idleAnimation.name);
				}
				else if (_characterState == PickupCharacterState.Running)
				{
					_animation[runAnimation.name].speed = runMaxAnimationSpeed;
					if (isControllable)
					{
						_animation[runAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0f, runMaxAnimationSpeed);
					}
					_animation.CrossFade(runAnimation.name);
				}
				else if (_characterState == PickupCharacterState.Trotting)
				{
					_animation[walkAnimation.name].speed = trotMaxAnimationSpeed;
					if (isControllable)
					{
						_animation[walkAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0f, trotMaxAnimationSpeed);
					}
					_animation.CrossFade(walkAnimation.name);
				}
				else if (_characterState == PickupCharacterState.Walking)
				{
					_animation[walkAnimation.name].speed = walkMaxAnimationSpeed;
					if (isControllable)
					{
						_animation[walkAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0f, walkMaxAnimationSpeed);
					}
					_animation.CrossFade(walkAnimation.name);
				}
				if (_characterState != PickupCharacterState.Running)
				{
					_animation[runAnimation.name].time = 0f;
				}
			}
		}
		if (IsGrounded() && DoRotate)
		{
			base.transform.rotation = Quaternion.LookRotation(moveDirection);
		}
		if (IsGrounded())
		{
			lastGroundedTime = Time.time;
			inAirVelocity = Vector3.zero;
			if (jumping)
			{
				jumping = false;
				SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
		lastPos = base.transform.position;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(base.transform.position);
			stream.SendNext((byte)_characterState);
			return;
		}
		bool num = remotePosition == Vector3.zero;
		remotePosition = (Vector3)stream.ReceiveNext();
		_characterState = (PickupCharacterState)(byte)stream.ReceiveNext();
		if (num)
		{
			base.transform.position = remotePosition;
		}
	}

	private void UpdateSmoothedMovementDirection()
	{
		Transform obj = Camera.main.transform;
		bool flag = IsGrounded();
		Vector3 vector = obj.TransformDirection(Vector3.forward);
		vector.y = 0f;
		vector = vector.normalized;
		Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
		float axisRaw = Input.GetAxisRaw("Vertical");
		float axisRaw2 = Input.GetAxisRaw("Horizontal");
		if (axisRaw < -0.2f)
		{
			movingBack = true;
		}
		else
		{
			movingBack = false;
		}
		bool flag2 = isMoving;
		isMoving = Mathf.Abs(axisRaw2) > 0.1f || Mathf.Abs(axisRaw) > 0.1f;
		Vector3 vector3 = axisRaw2 * vector2 + axisRaw * vector;
		if (flag)
		{
			lockCameraTimer += Time.deltaTime;
			if (isMoving != flag2)
			{
				lockCameraTimer = 0f;
			}
			if (vector3 != Vector3.zero)
			{
				if (moveSpeed < walkSpeed * 0.9f && flag)
				{
					moveDirection = vector3.normalized;
				}
				else
				{
					moveDirection = Vector3.RotateTowards(moveDirection, vector3, rotateSpeed * ((float)Math.PI / 180f) * Time.deltaTime, 1000f);
					moveDirection = moveDirection.normalized;
				}
			}
			float t = speedSmoothing * Time.deltaTime;
			float num = Mathf.Min(vector3.magnitude, 1f);
			_characterState = PickupCharacterState.Idle;
			if ((Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift)) && isMoving)
			{
				num *= runSpeed;
				_characterState = PickupCharacterState.Running;
			}
			else if (Time.time - trotAfterSeconds > walkTimeStart)
			{
				num *= trotSpeed;
				_characterState = PickupCharacterState.Trotting;
			}
			else if (isMoving)
			{
				num *= walkSpeed;
				_characterState = PickupCharacterState.Walking;
			}
			moveSpeed = Mathf.Lerp(moveSpeed, num, t);
			if (moveSpeed < walkSpeed * 0.3f)
			{
				walkTimeStart = Time.time;
			}
		}
		else
		{
			if (jumping)
			{
				lockCameraTimer = 0f;
			}
			if (isMoving)
			{
				inAirVelocity += vector3.normalized * Time.deltaTime * inAirControlAcceleration;
			}
		}
	}

	private void ApplyJumping()
	{
		if (!(lastJumpTime + jumpRepeatTime > Time.time) && IsGrounded() && canJump && Time.time < lastJumpButtonTime + jumpTimeout)
		{
			verticalSpeed = CalculateJumpVerticalSpeed(jumpHeight);
			SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void ApplyGravity()
	{
		if (isControllable)
		{
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0f)
			{
				jumpingReachedApex = true;
				SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
			if (IsGrounded())
			{
				verticalSpeed = 0f;
			}
			else
			{
				verticalSpeed -= gravity * Time.deltaTime;
			}
		}
	}

	private float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		return Mathf.Sqrt(2f * targetJumpHeight * gravity);
	}

	private void DidJump()
	{
		jumping = true;
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
		lastJumpButtonTime = -10f;
		_characterState = PickupCharacterState.Jumping;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		_ = hit.moveDirection.y;
		_ = 0.01f;
	}

	public float GetSpeed()
	{
		return moveSpeed;
	}

	public bool IsJumping()
	{
		return jumping;
	}

	public bool IsGrounded()
	{
		return (collisionFlags & CollisionFlags.Below) != 0;
	}

	public Vector3 GetDirection()
	{
		return moveDirection;
	}

	public bool IsMovingBackwards()
	{
		return movingBack;
	}

	public float GetLockCameraTimer()
	{
		return lockCameraTimer;
	}

	public bool IsMoving()
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
	}

	public bool HasJumpReachedApex()
	{
		return jumpingReachedApex;
	}

	public bool IsGroundedWithTimeout()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}

	public void Reset()
	{
		base.gameObject.tag = "Player";
	}
}
