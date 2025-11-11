using Fusion;
using UnityEngine;

public class BridgeController : NetworkBehaviour
{
    [Networked]
    public bool IsVisible { get; set; }

    [SerializeField] private GameObject visualRoot;

    public override void Spawned()
    {
        // Apenas a autoridade inicializa o estado
        if (Object.HasStateAuthority)
            IsVisible = false;

        UpdateVisibility();
    }

    public override void Render()
    {
        // Atualiza a visibilidade em todos os peers
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (visualRoot != null)
            visualRoot.SetActive(IsVisible);
    }

    // Chamado apenas pela autoridade da ponte
    public void ToggleBridge()
    {
        if (Object.HasStateAuthority)
        {
            IsVisible = !IsVisible;
            UpdateVisibility();
            Debug.Log("[BRIDGE] Ponte agora " + (IsVisible ? "visível" : "oculta"));
        }
    }
}
