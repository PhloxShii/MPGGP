using UnityEngine;

public class Player : MonoBehaviour
{
	[Header("Base setup")]
	public float walkingSpeed = 7.5f;

	public float runningSpeed = 7.5f;

	public float gravity = 20f;

	public float lookSpeed = 2f;

	public float lookXLimit = 45f;

	private CharacterController characterController;

	private Vector3 moveDirection = Vector3.zero;

	private float rotationX;

	[HideInInspector]
	public bool canMove = true;

	[SerializeField]
	private float cameraYOffset = 0.4f;

	private Camera playerCamera;

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		playerCamera = Camera.main;
		Cursor.visible = true;
	}

	private void Update()
	{
		bool flag = false;
		flag = Input.GetKey(KeyCode.LeftShift);
		Vector3 vector = base.transform.TransformDirection(Vector3.forward);
		Vector3 vector2 = base.transform.TransformDirection(Vector3.right);
		float num = (canMove ? ((flag ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical")) : 0f);
		float num2 = (canMove ? ((flag ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal")) : 0f);
		_ = moveDirection;
		moveDirection = vector * num + vector2 * num2;
		if (!characterController.isGrounded)
		{
			moveDirection.y -= gravity * Time.deltaTime;
		}
		characterController.Move(moveDirection * Time.deltaTime);
		_ = canMove;
		rotationX += (0f - Input.GetAxis("Mouse Y")) * lookSpeed;
		rotationX = Mathf.Clamp(rotationX, 0f - lookXLimit, lookXLimit);
		base.transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
	}
}
