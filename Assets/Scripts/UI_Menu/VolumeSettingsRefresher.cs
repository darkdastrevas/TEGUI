using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsRefresher : MonoBehaviour
{
    [Header("Qual Slider controla este volume?")]
    public Slider volumeSlider;

    [Header("Qual parâmetro do AudioMixer? (Ex: MasterVolume / MusicVolume / SFXVolume)")]
    public string mixerParameter = "MusicVolume";

    [Header("AudioMixer ligado ao sistema de áudio")]
    public UnityEngine.Audio.AudioMixer audioMixer;

    private bool initialized = false;

    void Start()
    {
        Reconnect();
    }

    void OnEnable()
    {
        // garante reconexão se o objeto ativar tardiamente
        if (!initialized) Reconnect();
    }

    private void Reconnect()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning($"[VolumeSettingsRefresher] Nenhum slider atribuído no inspetor para '{mixerParameter}'.");
            return;
        }

        initialized = true;

        // Remove listeners antigos para evitar duplicação
        volumeSlider.onValueChanged.RemoveAllListeners();

        // Reconecta o listener que ajusta o audio mixer
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Carrega valor salvo
        float savedVolume = PlayerPrefs.GetFloat(mixerParameter, 0.75f);
        volumeSlider.value = savedVolume;

        // Aplica no AudioMixer
        ApplyVolume(savedVolume);

        Debug.Log($"[VolumeSettingsRefresher] Slider reconectado ao volume '{mixerParameter}'.");
    }

    private void SetVolume(float sliderValue)
    {
        ApplyVolume(sliderValue);
        PlayerPrefs.SetFloat(mixerParameter, sliderValue);
    }

    private void ApplyVolume(float value)
    {
        // Converte linear → dB (padrão Unity)
        float db = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        audioMixer.SetFloat(mixerParameter, db);
    }
}
