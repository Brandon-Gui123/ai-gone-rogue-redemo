using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerPreferences : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("VSync")]
    public Toggle vSyncToggle;

    // Start is called before the first frame update
    private void Start()
    {
        // load volume levels from player prefs
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        audioMixer.SetFloat("MusicVolume", musicVolumeSlider.value);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        audioMixer.SetFloat("SFXVolume", sfxVolumeSlider.value);
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSyncCount") >= 1;
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
    }

    public void SaveVSyncSettings()
    {
        PlayerPrefs.SetInt("VSyncCount", vSyncToggle.isOn ? 1 : 0);
    }
}
