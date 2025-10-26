using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

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

    public void DoTheThing()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            guards.Add(transform.GetChild(i).GetComponent<GuardStateMachine>());
        }
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
        float distance;
        RuntimeManager.StudioSystem.getParameterByName("Distance", out distance);
        Debug.Log("distance " + distance);


        
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
            AudioManager.instance.SetGlobalParameter("Distance", Mathf.Infinity, false);
        }
        else
        {
            //SET THE DISTANCE VALUE TO THIS BELOW
            // Mathf.Sqrt(minSqr);
            float number = Mathf.Min(Mathf.Sqrt(minSqr), 30f) / 30f * 100f;
            AudioManager.instance.SetGlobalParameter("Distance", number, false);
        Debug.Log(number);
        }
    }
}
