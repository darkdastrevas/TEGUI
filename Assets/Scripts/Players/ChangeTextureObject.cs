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
    public GameObject particleBPrefab;
    public GameObject particleCPrefab;
    public Transform particleSpawnPoint;

    [Header("Audio")]
    private AudioSource audioSource;

    // -------------------------------------------------------
    // NETWORKED STATES
    // -------------------------------------------------------

    // Índice da textura
    [Networked] private int CurrentTextureIndex { get; set; } = -1;

    // Impede alterações futuras
    [Networked] private NetworkBool textureAlreadyChanged { get; set; } = false;

    private int lastAppliedIndex = -1;

    // -------------------------------------------------------
    public override void Spawned()
    {
        if (CurrentTextureIndex >= 0)
        {
            ApplyTexture(CurrentTextureIndex);
            lastAppliedIndex = CurrentTextureIndex;
        }

        audioSource = GetComponent<AudioSource>();
        ApplyTexture(CurrentTextureIndex);
        lastAppliedIndex = CurrentTextureIndex;
    }

    // -------------------------------------------------------
    // RPC — executa no StateAuthority
    // -------------------------------------------------------
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcApplyTextureIndex(int index)
    {
        // Impede troca se já foi alterada antes
        if (textureAlreadyChanged)
        {
            if (debugMode)
                Debug.Log("[ChangeTextureObject] Tentativa bloqueada — textura já alterada antes.");
            return;
        }

        // Índice válido?
        if (index < 0 || index > 4) return;

        // Atualiza textura
        CurrentTextureIndex = index;

        // Marca como alterada para nunca mais trocar
        textureAlreadyChanged = true;

        // Incrementa contador global
        TextureCounterController.Incrementar();

        if (debugMode)
            Debug.Log("[ChangeTextureObject] Textura alterada pela primeira e única vez: " + index);
    }

    // -------------------------------------------------------
    // Aplicação LOCAL da textura
    // -------------------------------------------------------
    private void ApplyTexture(int index)
    {
        if (!targetRenderer) return;

        if (index < 0) return;

        switch (index)
        {
            case 0:
                targetRenderer.material.mainTexture = textureA;
                break;

            case 1:
                targetRenderer.material.mainTexture = textureB;
                SpawnParticle(particleBPrefab);
                break;

            case 2:
                targetRenderer.material.mainTexture = textureB1;
                SpawnParticle(particleBPrefab);
                break;

            case 3:
                targetRenderer.material.mainTexture = textureC;
                SpawnParticle(particleCPrefab);
                break;

            case 4:
                targetRenderer.material.mainTexture = textureC1;
                SpawnParticle(particleCPrefab);
                break;
        }

        // 🔊 Toca som se tiver AudioSource
        if (audioSource != null)
            audioSource.Play();

        if (debugMode)
            Debug.Log("[ChangeTextureObject] Textura aplicada localmente: " + index);
    }

    // -------------------------------------------------------
    private void SpawnParticle(GameObject fx)
    {
        if (fx == null) return;

        Vector3 pos = particleSpawnPoint != null ?
                      particleSpawnPoint.position :
                      transform.position;

        Instantiate(fx, pos, Quaternion.identity);
    }

    // -------------------------------------------------------
    public override void Render()
    {
        if (CurrentTextureIndex != lastAppliedIndex)
        {
            ApplyTexture(CurrentTextureIndex);
            lastAppliedIndex = CurrentTextureIndex;
        }
    }
}
