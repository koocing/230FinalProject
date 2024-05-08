using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera; // Reference to the player's camera

    private PlayerInput playerInput; // Input actions

    // Singleton instance
    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } }

    // Mouse sensitivity and pitch for vertical rotation
    public float mouseSens = 3.5f;
    private float cameraPitch = 0f;

    // Cursor lock settings
    private bool lockCursor = true;

    // Movement and gravity variables
    private float speed = 6f;
    private float gravity = -13f;
    private float velocityY;

    // Smooth time variables for movement and mouse input
    private float moveSmoothTime = 0.1f;
    private float mouseSmoothTime = 0.08f;
    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    private CharacterController characterController; // CharacterController component

    private void Awake()
    {
        // Ensure a single instance of the PlayerController (singleton pattern)
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        // Initialize PlayerInput and enable it
        playerInput = new PlayerInput();
        playerInput.Enable();

        // Lock and hide the cursor if lockCursor is true
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Get the CharacterController component attached to the player
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Update camera and movement every frame
        UpdateCamera();
        UpdateMovement();
    }

    private void UpdateCamera()
    {
        // Get mouse delta input
        Vector2 targetMouseDelta = playerInput.InGame.Look.ReadValue<Vector2>();

        // Smooth the mouse delta input
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        // Adjust the camera pitch (vertical rotation) and clamp it
        cameraPitch -= currentMouseDelta.y * mouseSens;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        // Apply vertical rotation to the camera
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        // Apply horizontal rotation to the player
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSens);
    }

    private void UpdateMovement()
    {
        // Get movement input
        Vector2 targetDir = playerInput.InGame.Move.ReadValue<Vector2>().normalized;

        // Smooth the movement input
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // Reset vertical velocity if grounded
        if (characterController.isGrounded)
        {
            velocityY = 0.0f;
        }

        // Apply gravity
        velocityY += gravity * Time.deltaTime;

        // Calculate movement velocity
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * speed + Vector3.up * velocityY;

        // Move the character
        characterController.Move(velocity * Time.deltaTime);
    }
}
