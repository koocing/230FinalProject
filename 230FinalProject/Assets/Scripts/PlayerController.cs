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

    private float speed = 6f;
    private float gravity = -13f;
    private float velocityY;

    private float moveSmoothTime = 0.3f;
    private float mouseSmoothTime = 0.1f;
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

        // if the cursor should be locked, lock it
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; // also hide the cursor
        }

        characterController = GetComponent<CharacterController>();
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

        cameraPitch -= currentMouseDelta.y * mouseSens; // inverse mouseDelta so up is actually up and down is actually down
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); // clamp y rotation within -90 and 90 so camera can't flip upside down

        playerCamera.localEulerAngles = Vector3.right * cameraPitch; // the player camera will only rotate up and down around the x-axis
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

        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * speed + Vector3.up * velocityY;

        characterController.Move(velocity * Time.deltaTime);
    }
}
