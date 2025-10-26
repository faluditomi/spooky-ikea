using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardChaseBehaviour : MonoBehaviour
{
    private GuardStateMachine myStateMachine;

    private PlayerDetection myPlayerDetection;

    private Coroutine delayCoroutine;

    private GameObject lastKnownPlayerPositionPrefab;
    private GameObject currentLastKnownPlayerPosition;

    private Transform player;

    [Tooltip("The delay between the guard reaching the player's last known position and returning to its patrol route.")]
    [SerializeField] private float delayDuration = 3f;

    private void Awake()
    {
        myStateMachine = GetComponent<GuardStateMachine>();

        myPlayerDetection = GetComponent<PlayerDetection>();

        lastKnownPlayerPositionPrefab = Resources.Load<GameObject>("Prefabs/Guard/LastKnownPlayerPosition");

        currentLastKnownPlayerPosition = Instantiate(lastKnownPlayerPositionPrefab, Vector2.zero, Quaternion.identity);

        currentLastKnownPlayerPosition.SetActive(false);
    }

    private void OnEnable()
    {
        myStateMachine.OnSwithToChase += TargetPlayer;
        myStateMachine.OnChasing += ChasePlayer;
    }

    private void OnDisable()
    {
        myStateMachine.OnSwithToChase -= TargetPlayer;
        myStateMachine.OnChasing -= ChasePlayer;
    }

    private void TargetPlayer()
    {
        if(delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);

            delayCoroutine = null;
        }

        myStateMachine.GetAgent().SetDestination(myStateMachine.GetPlayer().position);

        currentLastKnownPlayerPosition.transform.position = myStateMachine.GetPlayer().position;

        currentLastKnownPlayerPosition.SetActive(true);
    }

    private void ChasePlayer()
    {
        if(myPlayerDetection.IsPlayerInSight() || myPlayerDetection.IsPlayerMakingNoiseRun() || myPlayerDetection.IsPlayerMakingNoiseSneak())
        {
            TargetPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!myStateMachine.GetCurrentGuardState().Equals(GuardStateMachine.GuardState.Chasing))
        {
            return;
        }

        if(other.CompareTag("LastKnownPlayerPosition"))
        {
            delayCoroutine = StartCoroutine(MenacingDelayBehaviour());
        }

        if(other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
    private IEnumerator MenacingDelayBehaviour()
    {
        yield return new WaitForSeconds(delayDuration);

        myStateMachine.SetState(GuardStateMachine.GuardState.Patrolling);
    }
}