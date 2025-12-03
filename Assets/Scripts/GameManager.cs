using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Fusion;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu Scene Name")]
    public string menuSceneName = "MainMenu";

    [Header("Music")]
    public AudioSource musicSource;
    [Header("Music Fade Out")]
    public float fadeOutDuration = 1.5f;     // tempo do fade

    [Header("Victory Sound")]
    public AudioClip victoryClip;            // O som de vitória para ser arrastado no Inspector

    [Header("Victory")]
    public int requiredTextureCount = 4;     // Número necessário para vencer
    public GameObject victoryScreen;          // Tela de vitória
    public bool isVictoryTriggered = false;

    private bool isBusy = false;
    [HideInInspector] public bool IsGameActive = true;
    public GameObject defeatScreen;
    public bool isGameOverTriggered = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = FindObjectOfType<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenuESC();
        }
    }

    // --- [Métodos de RestartLevel e Shutdown - Omitidos para concisão, mas mantidos intactos] ---

    public void RestartLevel()
    {
        if (isBusy) return;
        isBusy = true;

        var runner = FindObjectOfType<NetworkRunner>();

        if (runner != null)
        {
            var sessionHandler = FindObjectOfType<GameSessionHandler>();

            if (sessionHandler != null)
            {
                // sessionHandler.RPC_InitiateRestart(); // Reativar se GameSessionHandler existe
                StartCoroutine(ShutdownAndRestartRoutine(runner));
            }
            else
            {
                Debug.LogWarning("[GameManager] GameSessionHandler não encontrado. Fazendo shutdown direto.");
                StartCoroutine(ShutdownAndRestartRoutine(runner));
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] NetworkRunner não encontrado. Reiniciando sem shutdown.");
            StartCoroutine(ShutdownAndRestartRoutine(null));
        }
    }

    private void RPC_InitiateRestart()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            StartCoroutine(ShutdownAndRestartRoutine(runner));
        }
    }

    public IEnumerator ShutdownAndRestartRoutine(NetworkRunner runner)
    {
        if (runner != null)
        {
            Debug.Log("[GameManager] Iniciando shutdown da sessão para todos os jogadores.");
            yield return runner.Shutdown();
            Debug.Log("[GameManager] Sessão encerrada. Preparando para abrir uma nova sessão.");
        }

        IsGameActive = true;
        isGameOverTriggered = false;

        yield return new WaitForSeconds(0.4f);

        string currentSceneName = SceneManager.GetActiveScene().name;
        yield return SceneManager.LoadSceneAsync(currentSceneName);

        isBusy = false;
        Debug.Log("[GameManager] Nível reiniciado com nova sessão.");
    }

    public void ReturnToMenuButton()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        IsGameActive = true;
        isGameOverTriggered = false;
        var runner = FindObjectOfType<NetworkRunner>();

        if (musicSource != null)
            StartCoroutine(FadeOutMusic()); // Mantenha o FadeOut normal para o Menu

        if (runner != null)
        {
            yield return runner.Shutdown();
        }
        yield return new WaitForSeconds(0.4f);
        yield return SceneManager.LoadSceneAsync(menuSceneName);
        isBusy = false;
    }


    public void HandleGameOver(GameObject defeatScreenObject)
    {
        if (isGameOverTriggered || isVictoryTriggered) return;

        isGameOverTriggered = true;

        IsGameActive = false;
        Debug.Log("Game Over: Ativando tela de derrota.");

        // Para a música (Fade Out normal)
        if (musicSource != null)
            StartCoroutine(FadeOutMusic());

        if (defeatScreenObject != null)
        {
            defeatScreenObject.SetActive(true);
        }
    }

    public void HandleVictory(GameObject victoryScreenObject)
    {
        if (isVictoryTriggered || isGameOverTriggered) return;

        isVictoryTriggered = true;
        IsGameActive = false;
        Debug.Log("Vitória! Preparando tela de vitória...");

        // CHAVE: Inicia a corrotina que FAZ O FADE OUT e CHAMA o DelayedVictory.
        if (musicSource != null)
        {
            StartCoroutine(FadeOutMusicAndPlayVictory(victoryScreenObject));
        }
        else
        {
            // Fallback se não houver AudioSource
            StartCoroutine(DelayedVictory(victoryScreenObject));
        }
    }

    // NOVO MÉTODO: Combina Fade Out e a lógica de vitória
    private IEnumerator FadeOutMusicAndPlayVictory(GameObject victoryScreenObject)
    {
        // 1. FAZ O FADE OUT
        yield return FadeOutMusicRoutine();

        // 2. TOCA O SOM DE VITÓRIA (depois que a música parou)
        if (musicSource != null && victoryClip != null)
        {
            
            Debug.Log("Som de vitória ativado após Fade Out.");
        }

        // 3. CONTINUA COM O DELAY DE VITÓRIA
        yield return DelayedVictory(victoryScreenObject);
    }

    // MÉTODO AJUSTADO: Agora é uma corrotina pública para ser chamada por FadeOutMusicAndPlayVictory
    private IEnumerator FadeOutMusicRoutine()
    {
        if (musicSource == null || musicSource.clip == null)
            yield break;

        float startVolume = musicSource.volume;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    // MÉTODO ORIGINAL, mantido para Game Over e Menu, que agora chama o FadeOutMusicRoutine
    private IEnumerator FadeOutMusic()
    {
        yield return FadeOutMusicRoutine();
    }

    private IEnumerator DelayedVictory(GameObject victoryScreenObject)
    {
        // Nota: O som de vitória já foi tocado no FadeOutMusicAndPlayVictory.

        float victoryDelay = 1.5f;
        yield return new WaitForSecondsRealtime(victoryDelay);

        if (victoryScreenObject != null)
        {
            victoryScreenObject.SetActive(true);
            musicSource.PlayOneShot(victoryClip);
            Debug.Log("Tela de vitória ativada.");
        }
    }

    private void ReturnToMenuESC()
    {
        if (isBusy) return;
        isBusy = true;

        StartCoroutine(ReturnToMenuRoutine());
    }
}