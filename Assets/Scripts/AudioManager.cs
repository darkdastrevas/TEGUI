using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------
    // Música
    // ----------------------------------------------------
    public void PlayMusic(AudioClip clip, float fadeTime = 0.8f)
    {
        if (clip == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeMusic(clip, fadeTime));
    }

    IEnumerator FadeMusic(AudioClip clip, float time)
    {
        float start = musicSource.volume;

        // fade out
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(start, 0, t / time);
            yield return null;
        }

        musicSource.clip = clip;
        musicSource.Play();

        // fade in
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume * masterVolume, t / time);
            yield return null;
        }

        musicSource.volume = musicVolume * masterVolume;
    }

    // ----------------------------------------------------
    // SFX (sons curtos)
    // ----------------------------------------------------
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    // ----------------------------------------------------
    // Ajustar volumes em runtime
    // ----------------------------------------------------
    public void SetMasterVolume(float v)
    {
        masterVolume = v;
        UpdateVolumes();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        UpdateVolumes();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = v;
        UpdateVolumes();
    }

    void UpdateVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
    }
}
