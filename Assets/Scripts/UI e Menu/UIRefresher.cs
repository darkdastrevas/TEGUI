using UnityEngine;
using UnityEngine.UI;
using Fusion; // Necessário para a lógica de rede/autoridade

public class UIRefresher : MonoBehaviour
{
    [Header("UI Global (Apenas na cena do Jogo)")]
    public GameObject currentDefeatScreen; // Tela de derrota

    [Header("Botões de Transição")]
    public Button restartButton;
    public Button returnToMenuButton;


    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance não encontrado. Verifique a cena inicial.");
            return;
        }

        // --- 1. CONFIGURAÇÃO DO GAMEMANAGER E BOTÕES ---

        SetupGameManagerUI();

    }

    // Método separado para configurar o GameManager
    private void SetupGameManagerUI()
    {
        // Atribui a referência da tela de derrota LOCAL ao GameManager persistente
        if (currentDefeatScreen != null)
        {
            GameManager.Instance.defeatScreen = currentDefeatScreen;
            // Garante que a tela de derrota comece desativada
            currentDefeatScreen.SetActive(false);
        }

        // Reconecta o botão de Reiniciar
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(GameManager.Instance.RestartLevel);
        }

        // Reconecta o botão de Voltar ao Menu
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(GameManager.Instance.ReturnToMenuButton);
        }
    }
}