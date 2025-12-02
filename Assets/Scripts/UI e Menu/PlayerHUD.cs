using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image skillIconImage;

    [Header("Ícones")]
    [SerializeField] private Sprite skillIconPlayer1;
    [SerializeField] private Sprite skillIconPlayer2;

    /// <summary>
    /// Define os ícones com base na tag do player (mais robusto que autoridade).
    /// </summary>
    public void SetPlayer(PlayerMovement player)
    {
        if (player == null || skillIconImage == null)
        {
            Debug.LogWarning("[PlayerHUD] Player ou Image nulo.");
            return;
        }

        string playerTag = player.gameObject.tag;
        SetIconByTag(playerTag);
    }

    // Versão compatível com PlayerMovementDefi
    public void SetPlayer(PlayerMovementDefi player)
    {
        if (player == null || skillIconImage == null)
        {
            Debug.LogWarning("[PlayerHUD] Player ou Image nulo.");
            return;
        }

        string playerTag = player.gameObject.tag;
        SetIconByTag(playerTag);
    }

    /// <summary>
    /// Método auxiliar para definir ícone por tag.
    /// </summary>
    private void SetIconByTag(string playerTag)
    {
        if (playerTag == "Player")
        {
            if (skillIconPlayer1 != null)
            {
                skillIconImage.sprite = skillIconPlayer1;
                Debug.Log("[PlayerHUD] Ícone definido para Player 1.");
            }
        }
        else if (playerTag == "Player2")
        {
            if (skillIconPlayer2 != null)
            {
                skillIconImage.sprite = skillIconPlayer2;
                Debug.Log("[PlayerHUD] Ícone definido para Player 2.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHUD] Tag inválida: " + playerTag + ". Ícone não definido.");
        }
    }
}