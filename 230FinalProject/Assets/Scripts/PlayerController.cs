using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main player controller class.
/// Is responsible for moving the player using WASD, moving the camera attached to the player using the mouse, jumping, and wall jumping
/// </summary>
public class PlayerController : MonoBehaviour
{
    public Transform playerCamera; // Reference to the camera's transform, used for rotating the player
    public Camera mainCamera; // Reference to the Camera component, used for adjusting the field of view (FOV)

    private PlayerInput playerInput; // Manages player input

    // Singleton pattern to ensure only one instance of PlayerController exists, not really used
    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } }

    public float mouseSens = 3.5f; // Mouse sensitivity
    private float cameraPitch = 0f; // Up and down rotation of the camera

    private bool lockCursor = true; // Locks the cursor in the game window

    private float baseSpeed = 6f; // Starting speed of the player
    private float maxSpeed = 10f; // Maximum speed after acceleration
    private float accelerationRate = 1.1f; // How fast the player speeds up
    private float decelerationRate = 8f; // How fast the player slows down

    public float currentSpeed; // Current speed of the player
    private float gravity = -15f; // Gravity force
    private float velocityY; // Vertical velocity of the player
    public float jumpForce = 8f; // Force applied when jumping
    public float wallJumpForce = 8f; // Extra force for wall jumps

    private float baseFov = 60f; // Normal FOV
    private float maxFov = 90f; // FOV when moving fast
    private float targetFov; // The FOV we are aiming for
    private float fovLerpSpeed = 5f; // Speed of FOV change. Used only in reseting FOV to baseFov

    private float moveSmoothTime = 0.1f; // Smooth movement time
    private float mouseSmoothTime = 0.01f; // Smooth mouse look time
    private float mouseMoveLimit = 5f; // Limit for mouse movement. Used for locking abrupt mouse movement changes
    private Vector2 currentDir = Vector2.zero; // Current direction of movement
    private Vector2 currentDirVelocity = Vector2.zero; // Velocity for smoothing movement direction
    private Vector2 currentMouseDelta = Vector2.zero; // Mouse movement delta
    private Vector2 currentMouseDeltaVelocity = Vector2.zero; // Velocity for smoothing mouse movement

    private bool isNearWall; // Is the player near a wall?
    private bool hasWallJumped; // Has the player wall jumped?
    private Vector3 wallNormal; // Normal of the wall collided with
    private CharacterController characterController; // CharacterController component for movement

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        // Initialize player input
        playerInput = new PlayerInput();
        playerInput.Enable();

        // Lock the cursor if needed
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();
        currentSpeed = baseSpeed; // Set starting speed to the base speed for proper calculations later
        targetFov = baseFov; // Set starting FOV
    }

    private void Update()
    {
        UpdateCamera(); // Update the camera rotation
        UpdateMovement(); // Update the player movement

        // Update the speed display in the UI
        UIManager.Instance.UpdateSpeed(currentSpeed);
    }

    private void UpdateCamera()
    {
        // Get mouse movement from new input system
        Vector2 targetMouseDelta = playerInput.InGame.Look.ReadValue<Vector2>();

        // Smooth mouse movement using Vector2.SmoothDamp
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        // Adjust camera pitch (up and down)
        cameraPitch -= currentMouseDelta.y * mouseSens; // Inverse the the mouse delta, as the mouse delta is already inversed
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); // Clamp pitch to prevent flipping camera

        // Apply camera pitch. Camera will rotate about the x-axis on the player
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        // Rotate player based on mouse movement. This makes sure the player foward normal is aligned with the forward direction of the camera
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSens);
    }

    private void UpdateMovement()
    {
        // Get movement input
        Vector2 targetDir = playerInput.InGame.Move.ReadValue<Vector2>().normalized;

        // Smooth movement input using same method as mouse smoothing
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // Handle jumping and wall jumping. If the player is on the ground, allow the player to jump
        if (characterController.isGrounded)
        {
            velocityY = 0.0f; // Reset vertical velocity
            hasWallJumped = false; // Reset wall jump bool when touching the ground

            if (playerInput.InGame.Jump.triggered)
            {
                velocityY = jumpForce; // Apply jump force
            }
        }
        // If the player is near a wall, has pressed the jump button, and hasn't already wall jumped since last touching the ground...
        else if (isNearWall && playerInput.InGame.Jump.triggered && !hasWallJumped)
        {
            // Apply wall jump force
            velocityY = jumpForce;
            Vector3 wallJumpDirection = wallNormal * wallJumpForce;
            characterController.Move(wallJumpDirection * Time.deltaTime);
            hasWallJumped = true; // Set to true so the player can't continually wall jump up a wall
        }

        // If moving forward and the player isn't moving their mouse too quickly...
        if (targetDir.y > 0 && Mathf.Abs(currentMouseDelta.x) < mouseMoveLimit)
        {
            currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed); // Accelerate the player
        }
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - decelerationRate * Time.deltaTime, baseSpeed); // Decelerate the player
        }

        // Apply gravity. += because gravity is already negative, otherwise the player would fly away... (Idk why but that took me way too long to figure out)
        velocityY += gravity * Time.deltaTime;

        // Calculate movement velocity
        // Adds both forward and horizontal movement vectors together, resulting in a single vector that points in the direction the player wants to move in
        // Is then multiplied by speed
        // And finally the up vector is added from the player jumping, or gravity
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * currentSpeed + Vector3.up * velocityY;

        // Apply calculated velocity to the player
        characterController.Move(velocity * Time.deltaTime);

        // If player is not standing still...
        if (targetDir != Vector2.zero)
        {
            // Update FOV when moving
            // Interpolates between baseFov and maxFov, with the t value being a normalized ratio between the current speed and the max speed
            // Ex. if the player's current speed is 8, half way between the baseSpeed and maxSpeed, t = 0.5
            // This took me forever to figure out
            targetFov = Mathf.Lerp(baseFov, maxFov, (currentSpeed - baseSpeed) / (maxSpeed - baseSpeed));
            mainCamera.fieldOfView = targetFov; // Set camera FOV to targetFov
        }
        else
        {
            // Reset FOV when not moving, interpolating between the current FOV and the baseFov
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, baseFov, fovLerpSpeed * Time.deltaTime);
        }
    }

    // If the player collides with something...
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if near a wall
        // This is done by checking the collided objects normal
        // If the normal's y is just about at 0, the collided object is probably a vertical wall
        if (hit.normal.y < 0.1f && hit.normal.y > -0.1f)
        {
            isNearWall = true; // The player is near a wall
            wallNormal = hit.normal; // Get the wall normal
        }
        else
        {
            isNearWall = false; // If collided object is not a vertical wall, don't let player wall jump
        }
    }
}
