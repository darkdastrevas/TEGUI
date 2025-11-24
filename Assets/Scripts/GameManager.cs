using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu Scene Name")]
    public string menuSceneName = "MainMenu";

    private bool isBusy = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------
    // RESTART LEVEL
    // -------------------------------
    public void RestartLevel()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(RestartRoutine());
    }

    private System.Collections.IEnumerator RestartRoutine()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
            runner.Shutdown();

        yield return new WaitForSeconds(0.4f);

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);

        isBusy = false;
    }

    // -------------------------------
    // RETURN TO MENU
    // -------------------------------
    public void ReturnToMenuButton()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(ReturnToMenuRoutine());
    }

    private System.Collections.IEnumerator ReturnToMenuRoutine()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
            runner.Shutdown();

        yield return new WaitForSeconds(0.4f);

        SceneManager.LoadScene(menuSceneName);

        isBusy = false;
    }

    [Header("UI Fim de Jogo")]
    public GameObject defeatScreen; // Arraste a tela de derrota aqui

    // -------------------------------
    // EVENTO DISPARADO PELO CountdownTimer
    // -------------------------------
    public void OnTimeExpired()
    {
        isBusy = false; // Bloqueia outras transições

        // 1. Mostrar a tela de derrota localmente
        if (defeatScreen != null)
        {
            defeatScreen.SetActive(true);
        }
       
    }
}


