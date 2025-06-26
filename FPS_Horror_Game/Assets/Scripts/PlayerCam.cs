using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCam : MonoBehaviour
{
    public float SenseX;
    public float SenseY;
    public Transform playerOrientation;

    private float xRotation;
    private float yRotation;
    private Vector2 lookInput;
    private InputActions inputActions;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //StartCoroutine(FetchPlay());
    }

    private void OnEnable()
    {
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.Enable();
        inputActions.Player.Enable(); // enable the whole action map
    }

    private void OnDisable()
    {
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.Disable();
        inputActions.Player.Disable();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();

        float mouseX = lookInput.x * Time.deltaTime * SenseX;
        float mouseY = lookInput.y * Time.deltaTime * SenseY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    IEnumerator FetchPlay()
    {
        yield return new WaitForSeconds(7f);
        playerOrientation = GameObject.Find("Player").transform.GetChild(1).GetComponent<Transform>();

    }
}

