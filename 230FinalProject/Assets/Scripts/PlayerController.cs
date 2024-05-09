using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera;

    private PlayerInput playerInput;

    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } }

    public float mouseSens = 3.5f;
    private float cameraPitch = 0f;

    private bool lockCursor = true;

    private float baseSpeed = 6f; // Base movement speed
    private float maxSpeed = 10f; // Maximum speed with acceleration
    private float accelerationRate = 1.1f; // Rate at which speed increases
    private float decelerationRate = 8f; // Rate at which speed decreases when not accelerating

    public float currentSpeed;
    private float gravity = -13f;
    private float velocityY;

    private float moveSmoothTime = 0.1f;
    private float mouseSmoothTime = 0.08f;
    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    private CharacterController characterController;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        playerInput = new PlayerInput();
        playerInput.Enable();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        characterController = GetComponent<CharacterController>();
        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        UpdateCamera();
        UpdateMovement();
    }

    private void UpdateCamera()
    {
        Vector2 targetMouseDelta = playerInput.InGame.Look.ReadValue<Vector2>();

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSens;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSens);
    }

    private void UpdateMovement()
    {
        Vector2 targetDir = playerInput.InGame.Move.ReadValue<Vector2>().normalized;
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        if (characterController.isGrounded)
        {
            velocityY = 0.0f;
        }

        // Adjust the speed based on forward movement and acceleration
        if (currentDir.y > 0 && Mathf.Abs(currentMouseDelta.x) < 1f) // Forward movement and not moving mouse too quickly
        {
            currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - decelerationRate * Time.deltaTime, baseSpeed);
        }

        velocityY += gravity * Time.deltaTime;

        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * currentSpeed + Vector3.up * velocityY;

        characterController.Move(velocity * Time.deltaTime);
    }
}
