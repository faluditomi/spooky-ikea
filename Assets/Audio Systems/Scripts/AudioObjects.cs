using UnityEngine;

public class AudioObjects : MonoBehaviour
{
    // Holds references to GameObjects that plays sound (has an emitter attatched), in case these need to be acessed to make something work.
    // Can be acessed from any other script.
    public static AudioObjects instance { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public GameObject machineDialysis { get; private set; }
    [field: SerializeField] public GameObject airconditioner { get; private set; }
    [field: SerializeField] public GameObject window { get; private set; }
    [field: SerializeField] public GameObject radio { get; private set; }
    [field: SerializeField] public GameObject machineEKG { get; private set; }
}
