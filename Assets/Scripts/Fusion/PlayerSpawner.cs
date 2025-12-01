using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject player1Prefab; // Tag = Player
    [SerializeField] private GameObject player2Prefab; // Tag = Player2

    public void PlayerJoined(PlayerRef player)
    {
        // Apenas o runner local tenta spawnar seu player
        if (player != Runner.LocalPlayer)
            return;

        // Obtém o número total de players na sessão
        int playerCount = Runner.SessionInfo.PlayerCount;

        GameObject prefabToSpawn = null;

        // Lógica baseada no número de players
        if (playerCount == 1)
        {
            prefabToSpawn = player1Prefab;
        }
        else if (playerCount == 2)
        {
            prefabToSpawn = player2Prefab;
        }
        else
        {
            // Opcional: Para mais de 2 players, você pode decidir o que fazer
            // Ex.: Não spawnar, spawnar um prefab padrão, ou logar um erro
            Debug.LogWarning("Mais de 2 players detectados. Não spawnando.");
            return;
        }

        // Spawna o prefab apropriado
        Runner.Spawn(
            prefabToSpawn,
            new Vector3(0, 1, 0),
            Quaternion.identity,
            player
        );
    }
}