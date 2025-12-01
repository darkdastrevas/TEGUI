using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Configurações")]
    public string gameSceneName = "Game"; // Cena do jogo
    public GameMode gameMode = GameMode.Host; // Host, Client ou Shared
    public int maxPlayers = 2;

    [Header("Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    public NetworkRunner Runner { get; private set; }
    private PlayerSpawner playerSpawner;

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

    // Inicia um jogo como Host
    public async void StartHost()
    {
        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "MyGame",
            Scene = SceneRef.FromIndex(SceneManager.GetSceneByName(gameSceneName).buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers
        };

        var result = await Runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log("Jogo iniciado como Host.");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Falha ao iniciar: " + result.ShutdownReason);
        }
    }

    // Junta um jogo como Client
    public async void JoinClient()
    {
        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "MyGame",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers
        };

        var result = await Runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log("Conectado como Client.");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Falha ao conectar: " + result.ShutdownReason);
        }
    }

    // Chamado quando a cena do jogo carrega
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    if (scene.name == gameSceneName && Runner != null)
    //    {
    //        // Registra o PlayerSpawner
    //        playerSpawner = FindObjectOfType<PlayerSpawner>();
    //        if (playerSpawner != null)
    //        {
    //            Runner.AddCallbacks(playerSpawner);
    //            Debug.Log("PlayerSpawner registrado.");
    //        }
    //        else
    //        {
    //            Debug.LogError("PlayerSpawner não encontrado na cena!");
    //        }
    //    }
    //}

    //// Desconecta
    //public void Disconnect()
    //{
    //    if (Runner != null)
    //    {
    //        Runner.Shutdown();
    //        Debug.Log("Desconectado.");
    //    }
    //}

    //private void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //private void OnDisable()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}
}