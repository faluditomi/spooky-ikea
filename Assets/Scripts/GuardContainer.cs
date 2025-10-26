using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class GuardContainer : MonoBehaviour
{
    private List<GuardStateMachine> guards = new List<GuardStateMachine>();

    private Transform player;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            guards.Add(transform.GetChild(i).GetComponent<GuardStateMachine>());
        }

        player = FindFirstObjectByType<PlayerController>().transform;
    }

    void Start()
    {
        InvokeRepeating("DistanceToClosestEnemy", 0f, 0.5f);
    }

    public bool IsLastPursuer()
    {
        int i = 0;

        foreach (GuardStateMachine guard in guards)
        {
            if (guard.GetCurrentGuardState().Equals(GuardStateMachine.GuardState.Chasing))
            {
                i++;
            }
        }

        return i < 2;
    }
    
    public void DistanceToClosestEnemy()
    {
        if (guards.Count == 0)
        {
            return;
        }

        float minSqr = float.PositiveInfinity;
        Vector3 oPos = player.position;

        foreach (GuardStateMachine guard in guards)
        {
            if (guard == null) continue;
            Vector3 to = guard.transform.position - oPos;
            float sqr = to.sqrMagnitude;
            if (sqr < minSqr) minSqr = sqr;
        }

        if (float.IsPositiveInfinity(minSqr))
        {
            //SET THE DISTANCE VALUE TO THIS BELOW
            // Mathf.Infinity;
        }
        else
        {
            //SET THE DISTANCE VALUE TO THIS BELOW
            // Mathf.Sqrt(minSqr);
        }        
    }
}
