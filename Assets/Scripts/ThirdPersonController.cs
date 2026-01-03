using UnityEngine;

/// <summary>
/// Handles third-person character motion driven by WASD input.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.6f;
    [SerializeField] private float jogSpeed = 2.8f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Header("Grounding")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVelocity = -2f;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothDamp;
    private float rotationVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float inputMagnitude = Mathf.Clamp01(input.magnitude);
        bool wantsJog = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = (wantsJog ? jogSpeed : walkSpeed) * inputMagnitude;

        // Default to world axes when no main camera is present (e.g., tests or headless runs).
        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
            cameraRight = Camera.main.transform.right;
        }

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            float targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothRotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation,
                ref rotationVelocity,
                rotationSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, smoothRotation, 0f);
        }

        Vector3 targetVelocity = moveDirection.normalized * targetSpeed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmoothDamp, 1f / acceleration);

        if (controller.isGrounded)
        {
            // Keep the controller grounded without accumulating extra downward speed.
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedVelocity;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = currentVelocity + Vector3.up * verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }
}
