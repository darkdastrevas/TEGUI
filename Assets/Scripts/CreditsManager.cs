using UnityEngine;
using System.Collections;

public class CreditSequence : MonoBehaviour
{
    [System.Serializable]
    public class CreditScreen
    {
        public CanvasGroup group;
        public float fadeInTime = 1f;
        public float holdTime = 2f;
        public float fadeOutTime = 1f;
    }

    public CreditScreen[] screens; // coloque suas telas aqui no inspetor
    public GameObject creditsRoot; // objeto pai do canvas de créditos

    private void Start()
    {
        StartCoroutine(PlayCredits());
    }

    IEnumerator PlayCredits()
    {
        creditsRoot.SetActive(true);

        foreach (var screen in screens)
        {
            screen.group.gameObject.SetActive(true);

            // fade in
            yield return Fade(screen.group, 0, 1, screen.fadeInTime);

            // hold
            yield return new WaitForSeconds(screen.holdTime);

            // fade out
            yield return Fade(screen.group, 1, 0, screen.fadeOutTime);

            screen.group.gameObject.SetActive(false);
        }

        creditsRoot.SetActive(false);

        // se quiser chamar outra cena ou liberar player, faça aqui
        // SceneManager.LoadScene(...)
        // player.EnableMovement();
    }

    IEnumerator Fade(CanvasGroup g, float start, float end, float time)
    {
        float t = 0;
        g.alpha = start;

        while (t < time)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(start, end, t / time);
            yield return null;
        }

        g.alpha = end;
    }
}
