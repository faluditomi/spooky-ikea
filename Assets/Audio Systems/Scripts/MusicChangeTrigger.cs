using UnityEngine;

public class MusicChangeTrigger : MonoBehaviour
{
    
    [SerializeField] private MusicArea area;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Car"))
        {
            AudioManager.instance.SetMusicArea(area);
        }
    }
}
