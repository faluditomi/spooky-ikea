using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ActionEvent
{
    public string action;

    public UnityEvent onTrigger;
}

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private List<ActionEvent> actionEvents = new List<ActionEvent>();

    private Dictionary<string, UnityEvent> actionLookup;

    private void Awake()
    {
        actionLookup = new Dictionary<string, UnityEvent>();

        foreach(var pair in actionEvents)
        {
            if(pair != null && !string.IsNullOrEmpty(pair.action))
            {
                if(actionLookup.ContainsKey(pair.action))
                {
                    Debug.LogWarning($"Duplicate action string '{pair.action}' found on {gameObject.name}. The last entry in the list will be used.", this);
                }

                actionLookup[pair.action] = pair.onTrigger;
            }
        }
    }

    //Triggers Unity Events.
    public void Trigger(List<string> actionsToTrigger)
    {
        foreach(string actionToFire in actionsToTrigger)
        {
            if(actionLookup.TryGetValue(actionToFire, out UnityEvent eventToInkove))
            {
                eventToInkove?.Invoke();
            }
        }
    }
}
