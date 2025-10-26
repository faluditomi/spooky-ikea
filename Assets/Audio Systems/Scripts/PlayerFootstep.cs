using FMODUnity;
using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    
 
    [EventRef] public string footstepEvent;   // e.g. "event:/Player/Footstep"
    public float stepInterval = 0.5f;         // Time between footsteps
    public float moveThreshold = 0.1f;        // Minimum velocity to count as "moving"

    private float stepTimer = 0f;
    private PlayerController controller;   // Or Rigidbody, depending on movement system

    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        bool isMoving = controller != null && controller.velocity.magnitude > moveThreshold;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                RuntimeManager.PlayOneShot(footstepEvent, transform.position);
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f; // reset when not moving
        }
    }
  
}
