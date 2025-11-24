// UIRefresher.cs (CÓDIGO AJUSTADO)
using UnityEngine;
using UnityEngine.UI;

public class UIRefresher : MonoBehaviour
{
    [Header("Botões do Jogo/Menu")]
    public Button restartButton;
    public Button returnToMenuButton;

    [Header("UI Global (Apenas na cena do Jogo)")]
    // 🔑 NOVO CAMPO: Objeto da tela de derrota (arrastado no Inspector)
    public GameObject currentDefeatScreen;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(" GameManager.Instance não encontrado.");
            return;
        }

        // --- 1. CONFIGURAÇÃO DA UI GLOBAL ---

        // Atribui a referência da tela de derrota local ao GameManager persistente
        if (currentDefeatScreen != null)
        {
            GameManager.Instance.defeatScreen = currentDefeatScreen;
            // Garante que a tela de derrota comece desativada ao carregar a cena
            currentDefeatScreen.SetActive(false);
            Debug.Log("Tela de Derrota atribuída e desativada no GameManager.");
        }

        // --- 2. RECONEXÃO DOS BOTÕES ---

        // Reconecta o botão de Reiniciar (se existir na cena)
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(GameManager.Instance.RestartLevel);
            Debug.Log("Botão de Restart reconectado ao GameManager.");
        }

        // Reconecta o botão de Voltar ao Menu (se existir na cena)
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(GameManager.Instance.ReturnToMenuButton);
            Debug.Log("Botão de Retorno ao Menu reconectado ao GameManager.");
        }
    }
}