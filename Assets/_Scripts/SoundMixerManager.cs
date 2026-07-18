using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager Instance;

    public AudioMixer audioMixer;

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        SetMasterVolume(PlayerData.Instance.Data.MasterVolume);
        SetSoundFXVolume(PlayerData.Instance.Data.SoundFXVolume);
        SetMusicVolume(PlayerData.Instance.Data.MusicVolume);
    }

    // Range: 0.0001f to 1.0f
    public void SetMasterVolume(float volume)
    {
        float safeVolume = Mathf.Max(0.0001f, volume);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(safeVolume) * 20.0f);
        PlayerData.Instance.Data.MasterVolume = safeVolume;
    }

    // Range: 0.0001f to 1.0f
    public void SetSoundFXVolume(float volume)
    {
        float safeVolume = Mathf.Max(0.0001f, volume);
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(safeVolume) * 20.0f);
        PlayerData.Instance.Data.SoundFXVolume = safeVolume;
    }

    // Range: 0.0001f to 1.0f
    public void SetMusicVolume(float volume)
    {
        float safeVolume = Mathf.Max(0.0001f, volume);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(safeVolume) * 20.0f);
        PlayerData.Instance.Data.MusicVolume = safeVolume;
    }
}
