using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using Fusion; // Adicionado para acessar Runner

public class DefeatScreenController : MonoBehaviour
{
    [Header("Componentes de UI")]
    public CanvasGroup backgroundCanvasGroup;
    public GameObject buttonPanel;
    public VideoPlayer defeatVideoPlayer;
    public Button restartButton; // Novo: Referência ao botão de reiniciar

    [Header("Configurações de Tempo")]
    public float fadeInDuration = 1.5f;
    public float delayBeforeVideo = 0.5f;
    public float delayAfterVideo = 1.0f;
    public float fadeOutDuration = 1.0f;

    private bool sequenceStarted = false;
    private string localPlayerTag = ""; // Novo: Tag do jogador local

    private void Awake()
    {
        // Deixa TUDO desativado no começo
        backgroundCanvasGroup.alpha = 0f;
        buttonPanel.SetActive(false);
        defeatVideoPlayer.gameObject.SetActive(false);

        // MUITO IMPORTANTE → tela invisível e NÃO bloqueia cliques
        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameOverTriggered)
            return;

        // Novo: Determina a tag do jogador local
        DetermineLocalPlayerTag();

        sequenceStarted = false;
    }

    private void Update()
    {
        if (!sequenceStarted && GameManager.Instance != null && GameManager.Instance.isGameOverTriggered)
        {
            sequenceStarted = true;
            StartCoroutine(GameOverSequence());
        }
    }

    private void DetermineLocalPlayerTag()
    {
        // Encontra o jogador local (aquele com HasInputAuthority)
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in players)
        {
            if (player.HasInputAuthority)
            {
                localPlayerTag = player.gameObject.tag;
                Debug.Log("[DefeatScreenController] Jogador local identificado: " + localPlayerTag);
                return;
            }
        }

        // Se não encontrou PlayerMovement, tenta PlayerMovementDefi
        PlayerMovementDefi[] playersDefi = FindObjectsOfType<PlayerMovementDefi>();
        foreach (PlayerMovementDefi player in playersDefi)
        {
            if (player.HasInputAuthority)
            {
                localPlayerTag = player.gameObject.tag;
                Debug.Log("[DefeatScreenController] Jogador local identificado (Defi): " + localPlayerTag);
                return;
            }
        }

        Debug.LogWarning("[DefeatScreenController] Jogador local não encontrado. Usando tag padrão 'Player'.");
        localPlayerTag = "Player"; // Fallback
    }

    private IEnumerator GameOverSequence()
    {
        CanvasGroup parentCanvas = GetComponent<CanvasGroup>();

        // ATIVA O CANVAS AGORA, mas ainda invisível
        parentCanvas.alpha = 0f;
        parentCanvas.blocksRaycasts = true;

        // --- FADE IN DO FUNDO ---
        float t = 0;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = t / fadeInDuration;

            parentCanvas.alpha = a;
            backgroundCanvasGroup.alpha = a;

            yield return null;
        }

        parentCanvas.alpha = 1;
        backgroundCanvasGroup.alpha = 1;

        // Delay antes do vídeo
        yield return new WaitForSecondsRealtime(delayBeforeVideo);

        // --- TOCA VÍDEO ---
        if (defeatVideoPlayer != null)
        {
            defeatVideoPlayer.gameObject.SetActive(true);
            defeatVideoPlayer.Play();

            yield return new WaitForSecondsRealtime((float)defeatVideoPlayer.length);
        }

        yield return new WaitForSecondsRealtime(delayAfterVideo);

        // --- FADE OUT DO FUNDO ---
        t = 0;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = 1f - (t / fadeOutDuration);

            backgroundCanvasGroup.alpha = a;

            yield return null;
        }

        backgroundCanvasGroup.alpha = 0f;

        // --- MOSTRA OS BOTÕES ---
        Debug.Log("[DefeatScreenController] Ativando botões após fade out.");
        buttonPanel.SetActive(true);

        // Novo: Oculta o botão de reiniciar se for Player 2
        if (restartButton != null)
        {
            if (localPlayerTag == "Player2")
            {
                restartButton.gameObject.SetActive(false);
                Debug.Log("[DefeatScreenController] Botão de reiniciar ocultado para Player 2.");
            }
            else
            {
                restartButton.gameObject.SetActive(true);
                Debug.Log("[DefeatScreenController] Botão de reiniciar visível para Player 1.");
            }
        }
        else
        {
            Debug.LogWarning("[DefeatScreenController] restartButton não atribuído no Inspector.");
        }

        // Garantia de interatividade: Verifica e ativa CanvasGroup dos botões
        CanvasGroup buttonCanvas = buttonPanel.GetComponent<CanvasGroup>();
        if (buttonCanvas != null)
        {
            buttonCanvas.interactable = true;
            buttonCanvas.blocksRaycasts = true;
            Debug.Log("[DefeatScreenController] CanvasGroup dos botões configurado para interagível.");
        }
        else
        {
            Debug.LogWarning("[DefeatScreenController] buttonPanel não tem CanvasGroup. Adicione um para garantir interatividade.");
        }

        // Garante que o parentCanvas ainda permite cliques
        parentCanvas.blocksRaycasts = true;
    }
}
