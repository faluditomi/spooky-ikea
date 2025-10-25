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
    [SerializeField] private float crouchHeightMultiplier = 0.5f;
    [Tooltip("How long (seconds) the crouch/stand transition should take.")]
    [SerializeField] private float crouchTransitionDuration = 0.2f;

    // Internal state for smooth transitions
    private Coroutine crouchCoroutine;
    private Vector3 originalLocalScale;
    private bool isCrouched = false;

    void Awake()
    {
        input = new PlayerInputActions();

        myRigidbody = GetComponent<Rigidbody>();

        myStateMachine = GetComponent<PlayerStateMachine>();

        followTarget = transform.Find("Follow Target");

        // store the original scale so we can smoothly return to it
        originalLocalScale = transform.localScale;

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

        if(moveVector != null)
        {
            myRigidbody.AddRelativeForce(new Vector3(moveVector.x, 0f, moveVector.y) * myStateMachine.GetSpeedMultiplier(), ForceMode.VelocityChange);
        }

        if(moveVector.Equals(Vector3.zero))
        {
            if (!myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching))
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
        moveVector = input.ReadValue<Vector2>();
    }

    private void Sprint(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Sprinting);
        }
        else if (input.canceled)
        {
            myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
        }
    }

        //TODO: crouch
        //      -> lower view when crouching
        //      -> go under low things when crouching (like repo)
    private void Crouch(InputAction.CallbackContext input)
    {
        if(input.interaction is PressInteraction)
        {
            if(input.performed)
            {
                if (myStateMachine.GetCurrentState().Equals(PlayerStateMachine.PlayerState.Crouching))
                {
                    myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
                }
                else
                {
                    myStateMachine.SetState(PlayerStateMachine.PlayerState.Crouching);
                }
            }
        }
        else if(input.interaction is HoldInteraction)
        {
            if(input.performed)
            {
                myStateMachine.SetState(PlayerStateMachine.PlayerState.Crouching);
            }
            else if(input.canceled)
            {
                myStateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
            }
        }
    }

    private void Look(InputAction.CallbackContext input)
    {
        //The transform rotation can only be caught up to the follow transform's rotation
        //once the cinemachine scripts have had time to run.
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        //Once the player is rotated properly, we have to reset the follow target and thus
        //align the camera.
        followTarget.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        xRotation += -input.ReadValue<Vector2>().y * mouseSensitivity;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += input.ReadValue<Vector2>().x * mouseSensitivity;
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
        crouchCoroutine = StartCoroutine(ScaleTo(targetScale, crouchTransitionDuration, true));
    }

    public void StopCrouching()
    {
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
            crouchCoroutine = null;
        }

        crouchCoroutine = StartCoroutine(ScaleTo(originalLocalScale, crouchTransitionDuration, false));
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration, bool targetIsCrouched)
    {
        Vector3 startScale = transform.localScale;
        if (Mathf.Approximately(duration, 0f))
        {
            transform.localScale = targetScale;
            isCrouched = targetIsCrouched;
            crouchCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // smoothstep for nicer interpolation
            float eased = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, eased);
            yield return null;
        }

    transform.localScale = targetScale;
    isCrouched = targetIsCrouched;
    crouchCoroutine = null;
    }
}
