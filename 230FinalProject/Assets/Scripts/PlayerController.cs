using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } }

    public float speed = 5f;
    public float maxSpeed = 15f;
    public float acceleration = 0.1f;

    private Vector3 moveDirection = Vector3.zero;
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

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 input = playerInput.InGame.Move.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection.magnitude > 0)
        {
            speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);
        }
        else
        {
            speed = 5f; // Reset to initial speed if no movement
        }

        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    //public void OnMove(InputAction.CallbackContext context)
    //{
    //    Vector2 input = context.ReadValue<Vector2>();
    //    moveDirection = new Vector3(input.x, 0, input.y).normalized;
    //}
}
