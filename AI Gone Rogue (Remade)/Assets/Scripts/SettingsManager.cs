using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Space]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    [Space]
    public Toggle vsyncToggle;

    private void Start()
    {
        SetMusicVolume(musicVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetVSync(vsyncToggle.isOn);
    }

    public void SetMusicVolume(float volumeLevel)
    {
        audioMixer.SetFloat("MusicVolume", volumeLevel);
    }

    public void SetSFXVolume(float volumeLevel)
    {
        audioMixer.SetFloat("SFXVolume", volumeLevel);
    }

    public void SetVSync(bool useVSync)
    {
        QualitySettings.vSyncCount = (useVSync) ? 1 : 0;
    }
}
