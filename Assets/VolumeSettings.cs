using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer audioMixer;
    public string exposedParameter = "MusicVolume";

    [Header("UI")]
    public Slider volumeSlider;

    private const string SAVE_KEY = "MusicVolume";

    private void Start()
    {
        LoadVolume();
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    // -------------------------------------------------------
    // Ajusta o volume no AudioMixer
    // -------------------------------------------------------
    public void SetVolume(float sliderValue)
    {
        // Converte 0–1 para decibéis (-80 a 0)
        float dB = Mathf.Lerp(-80f, 0f, sliderValue);

        audioMixer.SetFloat(exposedParameter, dB);

        // Salva
        PlayerPrefs.SetFloat(SAVE_KEY, sliderValue);
    }

    // -------------------------------------------------------
    // Carrega volume do PlayerPrefs ou define padrão
    // -------------------------------------------------------
    void LoadVolume()
    {
        float saved = PlayerPrefs.GetFloat(SAVE_KEY, 1f);

        volumeSlider.value = saved;

        float dB = Mathf.Lerp(-40f, 0f, saved);
        audioMixer.SetFloat(exposedParameter, dB);
    }
}
