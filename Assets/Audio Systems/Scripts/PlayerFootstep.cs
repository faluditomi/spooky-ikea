using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    public float stepInterval = 0.5f;         // Time between footsteps
    public float moveThreshold = 0.1f;        // Minimum velocity to count as "moving"

    private float stepTimer = 0f;
    private Rigidbody rb;   // Or Rigidbody, depending on movement system
    private PlayerStateMachine playerStateMachine;
    private EventInstance playerFootstep;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        playerFootstep = AudioManager.instance.CreateEventInstance(FMODEvents.instance.playerFootstep);
    }

    void Update()
    {
        bool isMoving = rb.linearVelocity.magnitude > 0f;
        
        switch (playerStateMachine.GetCurrentState())
        {
            case PlayerStateMachine.PlayerState.Sneaking:
                stepInterval = 0.5f;
                playerFootstep.setVolume(0.6f);
                break;
            case PlayerStateMachine.PlayerState.Sprinting:
                stepInterval = 0.15f;
                playerFootstep.setVolume(1f);
                break;
            case PlayerStateMachine.PlayerState.Crouching:
                stepInterval = 1f;
                playerFootstep.setVolume(0f);
                break;
        }

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {

                RuntimeManager.PlayOneShot(FMODEvents.instance.playerFootstep, transform.position);
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval; // reset when not moving
        }
    }
  
}
