using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Fusion;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu Scene Name")]
    public string menuSceneName = "MainMenu";

    [Header("Victory")]
    public int requiredTextureCount = 4;   // Número necessário para vencer
    public GameObject victoryScreen;       // Tela de vitória
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
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenuESC();
        }
    }

    public void RestartLevel()
    {
        if (isBusy) return;
        isBusy = true;

        var runner = FindObjectOfType<NetworkRunner>();

        if (runner != null)
        {
            // Em shared mode, cada cliente pode iniciar o shutdown localmente.
            // Para sincronizar, notificamos todos os clientes via RPC para garantir que todos façam shutdown.
            var sessionHandler = FindObjectOfType<GameSessionHandler>(); // Substitua pelo seu objeto de rede (deve ser um NetworkBehaviour)

            if (sessionHandler != null)
            {
                // Chama o RPC no objeto de rede para notificar todos os clientes
                sessionHandler.RPC_InitiateRestart();
            }
            else
            {
                // Fallback: Se não houver handler, faz shutdown diretamente (não recomendado para shared mode)
                Debug.LogWarning("[GameManager] GameSessionHandler não encontrado. Fazendo shutdown direto.");
                StartCoroutine(ShutdownAndRestartRoutine(runner));
            }
        }
        else
        {
            // Standalone (sem rede)
            Debug.LogWarning("[GameManager] NetworkRunner não encontrado. Reiniciando sem shutdown.");
            StartCoroutine(ShutdownAndRestartRoutine(null));
        }
    }

    // RPC para notificar todos os clientes a fazerem shutdown (executado em todos os clientes)
    [Rpc(RpcSources.All, RpcTargets.All)] // Chamado por qualquer cliente, executado em todos
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
        // Shutdown da sessão (desconecta todos os players e encerra a game session)
        if (runner != null)
        {
            Debug.Log("[GameManager] Iniciando shutdown da sessão para todos os jogadores.");
            yield return runner.Shutdown(); // Isso desconecta o player local e sinaliza o fim da sessão para todos
            Debug.Log("[GameManager] Sessão encerrada. Preparando para abrir uma nova sessão.");
        }

        // Reseta flags
        IsGameActive = true;
        isGameOverTriggered = false;

        // Aguarda um pouco para garantir desconexão completa
        yield return new WaitForSeconds(0.4f);

        // Recarrega a cena (isso reinicia a lógica de spawning e pode iniciar uma nova sessão automaticamente)
        string currentSceneName = SceneManager.GetActiveScene().name;
        yield return SceneManager.LoadSceneAsync(currentSceneName);

        // Após recarregar, uma nova sessão pode ser iniciada (ex.: via PlayerSpawner ou outro script)
        // Se precisar forçar uma nova sessão aqui, adicione lógica para gerar um novo Room Name ou chamar runner.StartGame()

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
        isGameOverTriggered = false; // Reseta flag ao voltar ao menu
        var runner = FindObjectOfType<NetworkRunner>();
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
        if (isGameOverTriggered) return; // Evita múltiplas ativações
        isGameOverTriggered = true;

        IsGameActive = false;
        Debug.Log("Game Over: Ativando tela de derrota.");

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

        StartCoroutine(DelayedVictory(victoryScreenObject));
    }

    private IEnumerator DelayedVictory(GameObject victoryScreenObject)
    {
        // Delay antes da tela aparecer (ajuste à vontade)
        float victoryDelay = 2.5f;
        yield return new WaitForSecondsRealtime(victoryDelay);

        if (victoryScreenObject != null)
        {
            victoryScreenObject.SetActive(true);
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