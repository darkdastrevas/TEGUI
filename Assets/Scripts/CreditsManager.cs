using UnityEngine;
using System.Collections;

public class CreditsManager : MonoBehaviour
{
    [Header("Credit Screens (ordem)")]
    public CanvasGroup[] creditScreens;

    [Header("Background extra")]
    public CanvasGroup extraBackground;

    [Header("Timings")]
    public float fadeInTime = 1f;
    public float holdTime = 1.5f;
    public float fadeOutTime = 1f;

    [Header("Title Screen (sem fade)")]
    public GameObject titleScreenRoot;

    [Header("Flow")]
    public GameObject creditsRoot;

    // Flag estática para impedir repetição
    private static bool hasPlayed = false;

    private void Start()
    {
        // Ativa o título antes, mas invisível (se precisar)
        titleScreenRoot.SetActive(true);

        if (hasPlayed)
        {
            // Já tocou uma vez → pula direto pro título
            if (creditsRoot != null)
                creditsRoot.SetActive(false);

            if (extraBackground != null)
            {
                extraBackground.alpha = 0;
                extraBackground.gameObject.SetActive(false);
            }

            return;
        }

        hasPlayed = true; // marca como tocado

        // Créditos ativados
        if (creditsRoot != null)
            creditsRoot.SetActive(true);

        // Reseta telas
        foreach (var cg in creditScreens)
        {
            cg.alpha = 0;
            cg.gameObject.SetActive(false);
        }

        if (extraBackground != null)
        {
            extraBackground.alpha = 1;
            extraBackground.gameObject.SetActive(true);
        }

        StartCoroutine(RunCredits());
    }

    private IEnumerator RunCredits()
    {
        for (int i = 0; i < creditScreens.Length; i++)
        {
            CanvasGroup cg = creditScreens[i];

            cg.gameObject.SetActive(true);
            yield return Fade(cg, 0, 1, fadeInTime);
            yield return new WaitForSeconds(holdTime);
            yield return Fade(cg, 1, 0, fadeOutTime);
            cg.gameObject.SetActive(false);
        }

        // Funde fundo extra
        if (extraBackground != null)
        {
            yield return Fade(extraBackground, 1, 0, fadeOutTime);
            extraBackground.gameObject.SetActive(false);
        }

        // Agora só ativa o title screen SEM FADE
        if (titleScreenRoot != null)
            titleScreenRoot.SetActive(true);

        // Desliga os créditos
        if (creditsRoot != null)
            creditsRoot.SetActive(false);
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float time)
    {
        float t = 0;
        cg.alpha = from;

        while (t < time)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        cg.alpha = to;
    }
}
