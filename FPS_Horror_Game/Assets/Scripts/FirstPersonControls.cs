using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class FirstPersonControls : NetworkBehaviour
{
    [Header("Movement Settings")]
    [Space(5)]
    // Public variables to set movement and look speed, and the player camera
    public float moveSpeed; // Speed at which the player moves
    public float lookSpeed; // Sensitivity of the camera movement
    public float gravity = -9.81f; // Gravity value
    public float jumpHeight = 1.0f; // Height of the jump
    public Transform playerCamera; // Reference to the player's camera

    // Private variables to store input values and the character controller
    private Vector2 moveInput; // Stores the movement input from the player
    private Vector2 lookInput; // Stores the look input from the player
    private float verticalLookRotation = 0f; // Keeps track of vertical camera rotation for clamping
    private Vector3 velocity; // Velocity of the player

    private CharacterController characterController; // Reference to the CharacterController component
    private InputActions inputActions;
    private System.Action<InputAction.CallbackContext> onMovePerformed;
    private System.Action<InputAction.CallbackContext> onMoveCanceled;
    private System.Action<InputAction.CallbackContext> onLookPerformed;
    private System.Action<InputAction.CallbackContext> onLookCanceled;
    private System.Action<InputAction.CallbackContext> onJumpPerformed;
    private System.Action<InputAction.CallbackContext> onInteractPerformed;
    private System.Action<InputAction.CallbackContext> onSprintPerformed;
    private System.Action<InputAction.CallbackContext> onSprintCanceled;

    [Header("Sprint Settings")]
    [Space(5)]
    public float sprintMultiplier = 1.5f; // How much faster the player moves when sprinting

    private bool isSprinting = false;

    [Header("Interaction Settings")]
    [Space(5)]

    public float interactionRange;
    public LayerMask interactionLayer;
    
    [Header("HUD")]
    [Space(5)]
    public Canvas hudCanvas;
    private Color targetColor;
    public Image crosshairImage;
    public Color defaultColor;
    public Color hoverColor;



    private void Awake()
    {
        // Get and store the CharacterController component attached to this GameObject
        characterController = GetComponent<CharacterController>();
        inputActions = new InputActions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize delegates
        onInteractPerformed = ctx => Interaction();
        onMovePerformed = ctx => moveInput = ctx.ReadValue<Vector2>();
        onMoveCanceled = ctx => moveInput = Vector2.zero;
        onSprintPerformed = ctx => isSprinting = true;
        onSprintCanceled = ctx => isSprinting = false;
        onLookPerformed = ctx => lookInput = ctx.ReadValue<Vector2>();
        onLookCanceled = ctx => lookInput = Vector2.zero;
        onJumpPerformed = ctx => Jump();
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            hudCanvas.gameObject.SetActive(true);
            playerCamera.gameObject.SetActive(true);
        }
        else
        {
            hudCanvas.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Interact.performed += onInteractPerformed;

        inputActions.Player.Move.performed += onMovePerformed;
        inputActions.Player.Move.canceled += onMoveCanceled;

        inputActions.Player.Sprint.performed += onSprintPerformed;
        inputActions.Player.Sprint.canceled += onSprintCanceled;


        inputActions.Player.Look.performed += onLookPerformed;
        inputActions.Player.Look.canceled += onLookCanceled;

        inputActions.Player.Jump.performed += onJumpPerformed;
        
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= onInteractPerformed;

        inputActions.Player.Move.performed -= onMovePerformed;
        inputActions.Player.Move.canceled -= onMoveCanceled;

        inputActions.Player.Sprint.performed -= onSprintPerformed;
        inputActions.Player.Sprint.canceled -= onSprintCanceled;


        inputActions.Player.Look.performed -= onLookPerformed;
        inputActions.Player.Look.canceled -= onLookCanceled;

        inputActions.Player.Jump.performed -= onJumpPerformed;

        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        // Call Move and LookAround methods every frame to handle player movement and camera rotation
        Move();
        LookAround();
        ApplyGravity();
        CheckCrosshairHover();
    }

    private void Move()
    {
        Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));

        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    public void LookAround()
    {
        // Get horizontal and vertical look inputs and adjust based on sensitivity
        float LookX = lookInput.x * lookSpeed;
        float LookY = lookInput.y * lookSpeed;

        // Horizontal rotation: Rotate the player object around the y-axis
        transform.Rotate(0, LookX, 0);

        // Vertical rotation: Adjust the vertical look rotation and clamp it to prevent flipping
        verticalLookRotation -= LookY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        // Apply the clamped vertical rotation to the player camera
        playerCamera.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
    }

    public void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -0.5f; // Small value to keep the player grounded
        }

        velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
        characterController.Move(velocity * Time.deltaTime); // Apply the velocity to the character
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            // Calculate the jump velocity
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Interaction()
    {
        if (!IsOwner)
        {
            return;
        }
        
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        Debug.DrawRay(playerCamera.position, playerCamera.forward * interactionRange, Color.blue, 2f);
        print("searching");

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(); // works for door, pickup, NPC, etc.
            }
        }
    }

    

    private void CheckCrosshairHover()
    {
        if (crosshairImage == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer)
            && hit.collider.TryGetComponent<IInteractable>(out _))
        {
            targetColor = hoverColor;
        }
        else
        {
            targetColor = defaultColor;
        }

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);//smooth colour transition
    }
}
