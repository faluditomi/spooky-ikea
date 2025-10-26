using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class VolumeSlider : MonoBehaviour
{
    // Creates a new enumerator, that can be accessed from anywhere.
    // This is used to specify which Volume Type the slider is associated with.
    public enum VolumeType
    {
        MASTER,
        MUSIC,
        AMBIENCE,
        SFX
    }

    // Allows us to assign an enumerator to this slider, from the inspector.
    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;

    private void Awake()
    {
        // We get the Slider class componenent of this slider GameObject (this),
        // and store it in the volumeSlider variable.
        volumeSlider = this.GetComponentInChildren<Slider>();

        // TLDR: Saved volume settings are initialized.
        // We check to see if there is any data saved in the PlayerPrefs class, 
        // that uses the enumerator assigned to this slider, as it's key identifier.
        // If true, we retrieve it, and set the slider's value to the value retrieved.
        if(PlayerPrefs.HasKey(volumeType.ToString()))
        {
            volumeSlider.value = PlayerPrefs.GetFloat(volumeType.ToString());
        }
    }

    private void Update()
    {
        // If a different VolumeType is assigned to the slider,
        // the slider's value will be updated, with the last value stored, for that volume type.
        // Also ensure that the slider's value stays the same, until the "OnSliderValueCHanged()" method is run.
        switch(volumeType)
        {
            case VolumeType.MASTER:
                volumeSlider.value = MenuVolumeControls.instance.masterVolume;
            break;

            case VolumeType.MUSIC:
                volumeSlider.value = MenuVolumeControls.instance.musicVolume;
            break;

            case VolumeType.AMBIENCE:
                volumeSlider.value = MenuVolumeControls.instance.ambienceVolume;
            break;

            case VolumeType.SFX:
                volumeSlider.value = MenuVolumeControls.instance.sfxVolume;
            break;

            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
            break;  
        }
    }

    // Is called whenever a slide value changes.
    public void OnSliderValueChanged()
    {
        // We check which enumerator this slider is assigned, 
        // and set the associated volume float variable (defined in the MenuVolumeControls class) 
        // to the value of this slider.
        switch(volumeType)
        {
            case VolumeType.MASTER:
                MenuVolumeControls.instance.masterVolume = volumeSlider.value;
            break;

            case VolumeType.MUSIC:
                MenuVolumeControls.instance.musicVolume = volumeSlider.value;
            break;

            case VolumeType.AMBIENCE:
                MenuVolumeControls.instance.ambienceVolume = volumeSlider.value;
            break;

            case VolumeType.SFX:
                MenuVolumeControls.instance.sfxVolume = volumeSlider.value;
            break;

            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
            break;
        }

        // Saves the assigned enumerator string together with the value of the slider, into the PlayerPrefs unity class data.
        // This is saved locally on the player's device.
        // We can then retrieve the value later ("PlayerPrefs.GetFloat(string)"), using the enumerator string.
        PlayerPrefs.SetFloat(volumeType.ToString(), volumeSlider.value);
    }
}
