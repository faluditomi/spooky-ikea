using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    // This script holds references to all events in FMOD.

    // Sets the class itself to be static, meaning that even if the script is put on two different objects, so that two
    // "instances" of the class exist in the same scene, the content of those to instances is shared.
    // So whatever is changed in instance_01, is also changed in instance_02.
    // Also sets the contents of the class, to be readable from any other script, but can only be changed from inside the class itself.
    public static FMODEvents instance { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference roomtone { get; private set; }
    

    [field: Header("Music")]
    [field: SerializeField] public EventReference musicMenu { get; private set; }
    [field: SerializeField] public EventReference musicPhase01 { get; private set; }
    [field: SerializeField] public EventReference musicPhase02 { get; private set; }


    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootstep { get; private set; }
    [field: SerializeField] public EventReference playerPeeing { get; private set; }


    [field: Header("NPC SFX")]
    [field: SerializeField] public EventReference trollFootstep { get; private set; }
    [field: SerializeField] public EventReference trollNotice { get; private set; }


    [field: Header("Envrionment")]
    [field: SerializeField] public EventReference toiletOpen { get; private set; }


    [field: Header("UI")]
    [field: SerializeField] public EventReference mouseHover { get; private set; }
    [field: SerializeField] public EventReference mouseClick { get; private set; }
    [field: SerializeField] public EventReference valueChange { get; private set; }


    // Basically makes sure that only one instance of this class, will be taken into account,
    // in the case that more than one instance exists in the same scene. Not sure why this is important actually...
    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("Found more than one FMODEvents instance in the scene.");
        }
        instance = this;
    }
}
