using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //private PlayerInput playerInput;
    //public Transform playerBody;

    //private static CameraController _instance;
    //public static CameraController Instance { get { return _instance; } }

    //public float sensitivity = 100f;

    //float xRotation = 0f;

    //void Awake()
    //{
    //    if (_instance != null && _instance != this)
    //    {
    //        Destroy(this.gameObject);
    //    }
    //    else
    //    {
    //        _instance = this;
    //    }

    //    Cursor.lockState = CursorLockMode.Locked;
    //    playerInput = new PlayerInput();
    //    playerInput.Enable();
    //}

    //void Update()
    //{
    //    Vector2 lookInput = playerInput.InGame.Look.ReadValue<Vector2>() * sensitivity * Time.deltaTime;

    //    xRotation -= lookInput.y;
    //    xRotation = Mathf.Clamp(xRotation, -90f, 90f);

    //    // Apply rotation to the camera for vertical look
    //    transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    //    // Apply horizontal rotation to the player body
    //    playerBody.Rotate(Vector3.up * lookInput.x);
    //}
}
