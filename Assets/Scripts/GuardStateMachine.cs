using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GuardStateMachine : MonoBehaviour
{
    public enum GuardState
    {
        Patrolling,

        Chasing
    }
    [Tooltip("")]
    private GuardState currentState = GuardState.Patrolling;

    public event Action OnPatrolling;
    public event Action OnChasing;
    public event Action OnSwithToPatrol;
    public event Action OnSwithToChase;

    private GuardContainer guardContainer;

    private NavMeshAgent myAgent;

    private Transform player;

    private Light mainLight;

    private Color lightColor;

    [Tooltip("The speed at which the guard moves while patrolling.")]
    [SerializeField] private float patrolSpeed = 3f;
    [Tooltip("The speed at which the guard moves while it pursues the player.")]
    [SerializeField] private float chaseSpeed = 6f;
    
    private void Awake()
    {
        guardContainer = FindObjectOfType<GuardContainer>();

        myAgent = GetComponent<NavMeshAgent>();

        player = FindObjectOfType<PlayerController>().transform;

        mainLight = FindObjectOfType<Light>();
    }

    private void Start()
    {
        SetState(GuardState.Patrolling);

        lightColor = mainLight.color;
    }

    private void Update()
    {
        switch(currentState)
        {
            case GuardState.Patrolling:
                OnPatrolling?.Invoke();
            break;

            case GuardState.Chasing:
                OnChasing?.Invoke();
            break;

            default:
            break;
        }
    }

    public void SetState(GuardState state)
    {
        switch(state)
        {
            case GuardState.Patrolling:
                if(guardContainer.IsLastPursuer())
                {    
                    mainLight.color = lightColor;

                    mainLight.intensity = 1f;
                }

                currentState = GuardState.Patrolling;

                myAgent.speed = patrolSpeed;

                OnSwithToPatrol?.Invoke();
            break;

            case GuardState.Chasing:
                currentState = GuardState.Chasing;

                myAgent.speed = chaseSpeed;

                mainLight.color = Color.red;

                mainLight.intensity = 10f;

                OnSwithToChase?.Invoke();
            break;

            default:
            break;
        }
    }

    public GuardState GetCurrentGuardState()
    {
        return currentState;
    }

    public NavMeshAgent GetAgent()
    {
        return myAgent;
    }

    public Transform GetPlayer()
    {
        return player;
    }
}
