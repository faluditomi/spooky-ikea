using UnityEngine;

public class AmbienceChangeTrigger : MonoBehaviour
{
    // We can use a scipt like this in future projects, to quickly set up zones, where if the player enters something changes.
    // Very nice and easy for changes in ambience audio or music for instance.
    // Speculation: likely can also be used to load/unload/release specific EventInstances or FMOD sound banks, from memory,

    [Header("Parameter Change")]

    [SerializeField] private string parameterName;
    [SerializeField] private float parameterValue;

    // Checks to see if the player has collided with the zone, and then calls the SetAmbienceParameter method, from the AudioManager singleton,
    // setting a speciific parameter by a specific value, that are entered in the inspector.
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Car"))
        {
            AudioManager.instance.SetAmbienceParameter(parameterName, parameterValue);
        }
    }
}
