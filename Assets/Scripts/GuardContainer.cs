using System.Collections;
using System.Collections.Generic;
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
    
    public float DistanceToClosestEnemy()
    {
        if (guards.Count == 0) return Mathf.Infinity;

        float minSqr = float.PositiveInfinity;
        Vector3 oPos = player.position;

        foreach (GuardStateMachine guard in guards)
        {
            if (guard == null) continue;
            Vector3 to = guard.transform.position - oPos;
            float sqr = to.sqrMagnitude;
            if (sqr < minSqr) minSqr = sqr;
        }

        if (float.IsPositiveInfinity(minSqr)) return Mathf.Infinity;
        return Mathf.Sqrt(minSqr);
    }
}
