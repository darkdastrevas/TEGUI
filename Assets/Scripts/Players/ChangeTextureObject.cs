using Fusion;
using UnityEngine;

public class ChangeTextureObject : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Textures")]
    [SerializeField] private Texture textureA;
    [SerializeField] private Texture textureB;
    [SerializeField] private Texture textureB1;
    [SerializeField] private Texture textureC;
    [SerializeField] private Texture textureC1;

    [Header("Particles (FX)")]
    public GameObject particleBPrefab;   // FX quando aplica textura B ou B1
    public GameObject particleCPrefab;   // FX quando aplica textura C ou C1
    public Transform particleSpawnPoint; // Se vazio usa transform.position

    // Propriedade networked (sincroniza o índice)
    [Networked] private int CurrentTextureIndex { get; set; } = 0;

    private int lastAppliedIndex = -1;

    public override void Spawned()
    {
        ApplyTexture(CurrentTextureIndex);
        lastAppliedIndex = CurrentTextureIndex;
    }

    // RPC chamado por qualquer cliente, executado somente pelo Authority
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcApplyTextureIndex(int index)
    {
        if (index < 0 || index > 4) return;

        CurrentTextureIndex = index;

        TextureCounterController.Incrementar();

        if (debugMode)
            Debug.Log("[ChangeTextureObject] Índice de textura atualizado para: " + index);
    }

    // Aplica texture + partículas
    private void ApplyTexture(int index)
    {
        if (!targetRenderer) return;

        switch (index)
        {
            case 0: // Texture A
                targetRenderer.material.mainTexture = textureA;
                break;

            case 1: // Texture B
                targetRenderer.material.mainTexture = textureB;
                SpawnParticle(particleBPrefab);
                break;

            case 2: // Texture B1
                targetRenderer.material.mainTexture = textureB1;
                SpawnParticle(particleBPrefab);
                break;

            case 3: // Texture C
                targetRenderer.material.mainTexture = textureC;
                SpawnParticle(particleCPrefab);
                break;

            case 4: // Texture C1
                targetRenderer.material.mainTexture = textureC1;
                SpawnParticle(particleCPrefab);
                break;
        }

        if (debugMode)
            Debug.Log("[ChangeTextureObject] Textura aplicada: " + index);
    }

    // Instancia partículas localmente
    private void SpawnParticle(GameObject fx)
    {
        if (fx == null) return;

        Vector3 pos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;

        Instantiate(fx, pos, Quaternion.identity);
    }

    public override void Render()
    {
        if (CurrentTextureIndex != lastAppliedIndex)
        {
            ApplyTexture(CurrentTextureIndex);
            lastAppliedIndex = CurrentTextureIndex;
        }
    }
}
