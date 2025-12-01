using Fusion;
using UnityEngine;

public class GameSessionHandler : NetworkBehaviour
{
    // Singleton de sessão para fácil acesso pelo CountdownTimer
    public static GameSessionHandler ActiveHandler { get; private set; }

    [Header("UI da Cena")]
    // Referência para a tela de derrota LOCAL
    public GameObject localDefeatScreenUI;

    public override void Spawned()
    {
        // Registra este NetworkObject inicializado como o handler ativo da sessão
        ActiveHandler = this;

        // Garante que a tela de derrota local esteja desativada ao iniciar
        if (localDefeatScreenUI != null)
            localDefeatScreenUI.SetActive(false);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (ActiveHandler == this)
        {
            ActiveHandler = null;
        }
    }

    public void RPC_InitiateRestart()
    {
        // Este código só roda no Host (StateAuthority)

        // Despacha a chamada para o GameManager (que está na cena e é DontDestroyOnLoad)
        if (GameManager.Instance != null)
        {
            Debug.Log("[GameSessionHandler] Host recebeu RPC. Iniciando rotina no GameManager.");
            // O Host chama diretamente a rotina, passando seu próprio Runner
            GameManager.Instance.StartCoroutine(
                GameManager.Instance.ShutdownAndRestartRoutine(Runner)
            );
        }
    }

    // Este RPC é chamado SOMENTE pela StateAuthority (o Host) quando o timer zera.
    // O Fusion garante que ele seja executado em TODOS os clientes (RpcTargets.All).
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GameOver()
    {
        Debug.Log("RPC_GameOver recebido em todos os clientes. Ativando Game Over local.");

        if (GameManager.Instance != null)
        {
            // O GameManager, que é local, executa a ação e recebe a referência de UI
            GameManager.Instance.HandleGameOver(localDefeatScreenUI);
        }
    }
}