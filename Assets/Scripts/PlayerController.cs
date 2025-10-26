using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions input;

    private PlayerStateMachine myStateMachine;

    private Rigidbody myRigidbody;

    private Transform followTarget;

    private Vector2 moveVector = Vector2.zero;

    private float yRotation = 0f;
    private float xRotation = 0f;
    [Tooltip("Sensitivity of mouse look rotation.")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    
    [Header("Crouch")]
    [Tooltip("Multiplier applied to the player's local Y scale when crouching.")]
    [SerializeField] private float crouchHeightMultiplier = 0.3f;
    [Tooltip("How long (seconds) the crouch/stand transition should take.")]
    [SerializeField] private float crouchTransitionDuration = 0.2f;
    [Tooltip("Layers that block the player from standing up")]
    [SerializeField] private LayerMask standBlockLayers;

    // Internal state for smooth transitions
    private Coroutine crouchCoroutine;
    private Vector3 originalLocalScale;
    private Collider myCollider;

    private bool isMovementLocked = false;
    private bool isCameraLocked = false;

    void Awake()
    {
        input = new PlayerInputActions();

        myRigidbody = GetComponent<Rigidbody>();

        myStateMachine = GetComponent<PlayerStateMachine>();

        followTarget = transform.Find("Follow Target");

        // store the original scale so we can smoothly return to it
        originalLocalScale = transform.localScale;

        myCollider = GetComponent<Collider>();

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }

    void OnEnable()
    {
        input.Enable();

        input.Gameplay.Movement.performed += Move;
        input.Gameplay.Movement.canceled += Move;

        input.Gameplay.Sprint.performed += Sprint;
        input.Gameplay.Sprint.canceled += Sprint;

        input.Gameplay.Look.performed += Look;
        input.Gameplay.Look.canceled += Look;

        input.Gameplay.Crouch.performed += Crouch;
        input.Gameplay.Crouch.canceled += Crouch;
    }

    void OnDisable()
    {
        input.Disable();

        input.Gameplay.Movement.performed -= Move;
        input.Gameplay.Movement.canceled -= Move;

        input.Gameplay.Sprint.performed -= Sprint;
        input.Gameplay.Sprint.canceled -= Sprint;

        input.Gameplay.Look.performed -= Look;
        input.Gameplay.Look.canceled -= Look;

        input.Gameplay.Crouch.performed -= Crouch;
        input.Gameplay.Crouch.canceled -= Crouch;
    }

    private void FixedUpdate()
    {
        followTarget.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if(moveVector != null && !isMovementLocked)
        {
            myRigidbody.AddRelativeForce(new Vector3(moveVector.x, 0f, moveVector.y) * myStateMachine.GetSpeedMultiplier(), ForceMode.VelocityChange);
        }

        if(moveVector.Equals(Vector3.zero))
        {
            if(!myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching) && !myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Sprinting))
            {
                myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
            }
        }
        else
        {
            if(myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Idle))
            {
                myStateMachine.SetState(PlayerStateMachine.PlayerState.Sneaking);
            }

        }
    }

    private void Move(InputAction.CallbackContext input)
    {
        if(isMovementLocked)
        {
            moveVector = Vector2.zero;

            return;
        }

        moveVector = input.ReadValue<Vector2>();
    }

    private void Sprint(InputAction.CallbackContext input)
    {
        if(isMovementLocked || myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching)) return;
        if (input.performed)
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Sprinting);
        }
        else if (input.canceled)
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
        }
    }

    private void Crouch(InputAction.CallbackContext input)
    {
        if(isMovementLocked) return;
        if(input.interaction is PressInteraction)
        {
            if(input.performed)
            {
                if (myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching))
                {
                    StopCrouching();
                }
                else
                {
                    StartCrouching();
                }
            }
        }
        else if(input.interaction is HoldInteraction)
        {
            if(input.performed)
            {
                StartCrouching();
            }
            else if(input.canceled)
            {
                StopCrouching();
            }
        }
    }

    private void Look(InputAction.CallbackContext input)
    {
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        followTarget.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        if(isCameraLocked) return;

        xRotation += -input.ReadValue<Vector2>().y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        yRotation += input.ReadValue<Vector2>().x * mouseSensitivity;
    }

    public void LockMovement(bool lockCamera)
    {
        isMovementLocked = true;
        isCameraLocked = lockCamera;
        moveVector = Vector2.zero;
        myRigidbody.linearVelocity = new Vector3(0f, myRigidbody.linearVelocity.y, 0f);

        if(!myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching))
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
        }
    }

    public void UnlockMovement()
    {
        isMovementLocked = false;
        isCameraLocked = false;
    }

    public void StartCrouching()
    {
        // If already crouched or a crouch transition is running, cancel it and start a new one.
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }

        Vector3 targetScale = new Vector3(originalLocalScale.x, originalLocalScale.y * crouchHeightMultiplier, originalLocalScale.z);
        crouchCoroutine = StartCoroutine(ScaleTo(targetScale, crouchTransitionDuration, false));
    }

    public void StopCrouching()
    {
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }

        crouchCoroutine = StartCoroutine(ScaleTo(originalLocalScale, crouchTransitionDuration, true));
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration, bool targetIsCrouched)
    {
        if (targetIsCrouched)
        {
            // Cast a ray from the player's head upward to check for obstacles
            RaycastHit hit;
            yield return new WaitUntil(() => !Physics.Raycast(transform.position, Vector3.up, out hit, 2f, standBlockLayers));
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
        }
        else
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Crouching);
        }

        Vector3 startScale = transform.localScale;

        // Get initial collider bounds before scaling
        float startExtentY = myCollider.bounds.extents.y;
        float startBottomY = myCollider.bounds.center.y - startExtentY;

        if (Mathf.Approximately(duration, 0f))
        {
            transform.localScale = targetScale;
            crouchCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothstep easing
            float eased = t * t * (3f - 2f * t);

            // Interpolate scale
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, eased);
            transform.localScale = newScale;

            // --- Keep feet grounded ---
            // Compute new collider bottom after scaling
            float newExtentY = myCollider.bounds.extents.y;
            float newBottomY = myCollider.bounds.center.y - newExtentY;

            // Compute the vertical offset needed to keep the feet in place
            float bottomDelta = startBottomY - newBottomY;
            transform.position += new Vector3(0f, bottomDelta, 0f);

            yield return null;
        }

        transform.localScale = targetScale;

        // Final correction for any accumulated floating-point error
        float finalExtentY = myCollider.bounds.extents.y;
        float finalBottomY = myCollider.bounds.center.y - finalExtentY;
        float finalDelta = startBottomY - finalBottomY;
        transform.position += new Vector3(0f, finalDelta, 0f);

        crouchCoroutine = null;
    }
}
