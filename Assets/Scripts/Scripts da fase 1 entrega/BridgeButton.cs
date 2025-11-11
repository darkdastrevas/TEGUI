using Fusion;
using UnityEngine;

public class BridgeButton : NetworkBehaviour
{
    [SerializeField] private BridgeController bridge;
    [Networked] public bool AlreadyUsed { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            AlreadyUsed = false;
    }

    // Chamado quando o jogador aperta o botão localmente
    public void TryUse()
    {
        if (AlreadyUsed)
        {
            Debug.Log("[BUTTON] Botão já foi usado!");
            return;
        }

        Debug.Log("[BUTTON] Tentando usar o botão...");
        RpcUseButton();
    }

    // RPC para autoridade do botão
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcUseButton()
    {
        if (AlreadyUsed)
        {
            Debug.Log("[BUTTON RPC] Botão já estava usado. Cancelando.");
            return;
        }

        AlreadyUsed = true;
        Debug.Log("[BUTTON RPC] Botão marcado como usado.");

        // A autoridade do botão pede à ponte para alternar seu estado
        if (bridge != null && bridge.Object.HasStateAuthority)
        {
            bridge.ToggleBridge();
        }
    }
}
