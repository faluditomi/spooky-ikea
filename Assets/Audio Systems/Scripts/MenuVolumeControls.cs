using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MenuVolumeControls : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)] public float masterVolume = 1;
    [Range(0, 1)] public float musicVolume = 1;
    [Range(0, 1)] public float ambienceVolume = 1;
    [Range(0, 1)] public float sfxVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus ambienceBus;
    private Bus sfxBus;

    public static MenuVolumeControls instance { get; private set; }

    void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }

        instance = this;

        masterBus = RuntimeManager.GetBus("bus:/");

        musicBus = RuntimeManager.GetBus("bus:/Music");

        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");

        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        if(PlayerPrefs.HasKey(VolumeSlider.VolumeType.MASTER.ToString()))
        {
            masterVolume = PlayerPrefs.GetFloat(VolumeSlider.VolumeType.MASTER.ToString());
        }

        if(PlayerPrefs.HasKey(VolumeSlider.VolumeType.MUSIC.ToString()))
        {
            musicVolume = PlayerPrefs.GetFloat(VolumeSlider.VolumeType.MUSIC.ToString());
        }

        if(PlayerPrefs.HasKey(VolumeSlider.VolumeType.AMBIENCE.ToString()))
        {
            ambienceVolume = PlayerPrefs.GetFloat(VolumeSlider.VolumeType.AMBIENCE.ToString());
        }

        if(PlayerPrefs.HasKey(VolumeSlider.VolumeType.SFX.ToString()))
        {
            sfxVolume = PlayerPrefs.GetFloat(VolumeSlider.VolumeType.SFX.ToString());
        }
    }

    private void Update()
    {
        masterBus.setVolume(masterVolume);

        musicBus.setVolume(musicVolume);

        ambienceBus.setVolume(ambienceVolume);

        sfxBus.setVolume(sfxVolume);
    }
}
