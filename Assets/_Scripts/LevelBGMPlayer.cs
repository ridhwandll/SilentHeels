using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelBGMPlayer : MonoBehaviour
{
    [Header("Audio Tracks")]
    public AudioClip levelBGM;
    public AudioClip bossBGM;

    [Header("Transition Settings")]
    public float transitionDuration = 1.5f;

    private AudioSource _audioSource;
    private float _defaultVolume;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _defaultVolume = _audioSource.volume;

        if (levelBGM != null)
        {
            _audioSource.clip = levelBGM;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    public void PlayBossMusic()
    {
        if (_audioSource.clip == bossBGM)
            return;

        if (bossBGM != null)
        {
            StartCoroutine(CrossfadeRoutine());
        }
    }

    private IEnumerator CrossfadeRoutine()
    {
        float halfDuration = transitionDuration / 2f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(_defaultVolume, 0f, timer / halfDuration);
            yield return null; // Wait for the next frame
        }

        _audioSource.volume = 0f;

        _audioSource.clip = bossBGM;
        _audioSource.Play();
        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, _defaultVolume, timer / halfDuration);
            yield return null;
        }

        _audioSource.volume = _defaultVolume;
    }
}