using Fusion;
using UnityEngine;

public class ChangeTextureObject : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [Header("Textures")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Texture textureA;

    [SerializeField] private Texture textureB;
    [SerializeField] private Texture textureB1;

    [SerializeField] private Texture textureC;
    [SerializeField] private Texture textureC1;

    [Networked] // Contador networked para que todos vejam
    public int Contador { get; private set; } = 0;

    public override void Spawned()
    {
        if (targetRenderer != null)
            targetRenderer.material.mainTexture = textureA;
    }

    // RPC: recebe o índice da textura
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcApplyTextureIndex(int index)
    {
        if (!targetRenderer) return;

        switch (index)
        {
            case 1: targetRenderer.material.mainTexture = textureB; break;
            case 2: targetRenderer.material.mainTexture = textureB1; break;
            case 3: targetRenderer.material.mainTexture = textureC; break;
            case 4: targetRenderer.material.mainTexture = textureC1; break;
        }

        // Incrementa o contador sempre que a textura muda
        Contador++;

        if (debugMode)
            Debug.Log("[ChangeTextureObject] Textura aplicada: " + index + " | Contador: " + Contador);
    }
}
