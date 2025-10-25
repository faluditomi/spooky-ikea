using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Sneaking,
        Sprinting,
        Crouching
    }

    private PlayerState currentState = PlayerState.Idle;

    private PlayerController playerController;

    [SerializeField] private float sneakSpeed = 1.2f;
    [Tooltip("The speed that the player moves at when they are sprinting and making noise.")]
    [SerializeField] private float sprintSpeed = 3f;
    [Tooltip("The speed that the player moves at when they are crouching or hiding under things.")]
    [SerializeField] private float crouchSpeed = 0.7f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        Debug.Log(currentState);
    }

    public void SetState(PlayerState state)
    {
        switch(currentState)
        {
            // case PlayerState.Sneaking:
            //     break;
            // case PlayerState.Sprinting:
            //     break;
            case PlayerState.Crouching:
                if(!state.Equals(PlayerState.Crouching))
                {
                    playerController.StopCrouching();
                }
                break;
            // case PlayerState.Idle:
            //     break;
        }
        
        switch(state)
        {
            //     case PlayerState.Sneaking:
            //         break;
            //     case PlayerState.Sprinting:
            //         break;
            case PlayerState.Crouching:
                if(!currentState.Equals(PlayerState.Crouching))
                {
                    playerController.StartCrouching();
                }
                break;
        //     case PlayerState.Idle:
        //         break;
        }

        currentState = state;
    }

    public PlayerState GetCurrentState()
    {
        return currentState;
    }

    public float GetSpeedMultiplier()
    {
        return currentState switch
        {
            PlayerState.Sneaking => sneakSpeed,
            PlayerState.Sprinting => sprintSpeed,
            PlayerState.Crouching => crouchSpeed,
            PlayerState.Idle => 0,
            _ => 0
        };
    }
}