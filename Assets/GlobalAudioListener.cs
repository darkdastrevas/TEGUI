using UnityEngine;
using Fusion;

public class GlobalAudioListener : NetworkBehaviour
{
    private AudioListener audioListener;
    private Camera playerCamera; // Opcional: Se a câmera estiver anexa

    public override void Spawned()
    {
        // Pega os componentes de áudio e câmera que estão no prefab do jogador
        audioListener = GetComponent<AudioListener>();
        playerCamera = GetComponentInChildren<Camera>(true); // Busca câmera mesmo se estiver desativada

        // A CHAVE É A AUTORIDADE DE INPUT
        // Verifica se este objeto de rede é aquele que o jogador local está controlando.
        if (Object.HasInputAuthority)
        {
            // É o jogador local. Ativa os componentes de áudio e visual.
            if (audioListener != null)
            {
                audioListener.enabled = true;
                Debug.Log("AudioListener ATIVADO para o Player Local.");
            }
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            // NÃO é o jogador local (é o jogador de outro cliente). Desativa.
            if (audioListener != null)
            {
                audioListener.enabled = false;
                Debug.Log("AudioListener DESATIVADO para o Player Remoto.");
            }
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
        }
    }
}