using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            GameObject prefabToSpawn = Runner.SessionInfo.PlayerCount == 1
                ? player1Prefab
                : player2Prefab;

            Runner.Spawn(prefabToSpawn, new Vector3(0, 1, 0), Quaternion.identity, player);
        }
    }
}
