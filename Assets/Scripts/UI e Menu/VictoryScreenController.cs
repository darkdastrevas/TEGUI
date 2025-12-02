using UnityEngine;
using UnityEngine.UI; // Para Button
using System.Collections; // Para Coroutine

public class VictoryScreenController : MonoBehaviour
{
    [Header("Victory Settings")]
    [SerializeField] private Button backToMenuButton; // Referência ao botão de voltar ao menu
    [SerializeField] private float delayAfterVictory = 5f; // Delay em segundos após a vitória (ajuste para o tempo do vídeo ou fixo)

    private bool victoryHandled = false; // Flag para evitar múltiplas ativações

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (TextureCounterController.Instance == null) return;
        if (!GameManager.Instance.IsGameActive) return; // Se o jogo acabou, não continua

        int currentCount = TextureCounterController.Instance.TextureChangeCount;
        int required = GameManager.Instance.requiredTextureCount;

        if (currentCount >= required && !victoryHandled)
        {
            victoryHandled = true; // Marca como tratado
            GameManager.Instance.HandleVictory(GameManager.Instance.victoryScreen);

            // Inicia a coroutine para ativar o botão após delay
            StartCoroutine(ActivateBackToMenuButtonAfterDelay());
        }
    }

    private IEnumerator ActivateBackToMenuButtonAfterDelay()
    {
        // Aguarda o delay (ou o tempo do vídeo)
        yield return new WaitForSeconds(delayAfterVictory);

        // Ativa o botão de voltar ao menu
        if (backToMenuButton != null)
        {
            backToMenuButton.gameObject.SetActive(true);
            backToMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenuButton());
            Debug.Log("[VictoryScreenController] Botão de voltar ao menu ativado.");
        }
        else
        {
            Debug.LogWarning("[VictoryScreenController] Botão de voltar ao menu não atribuído!");
        }
    }
}



//using UnityEngine;

//public class VictoryScreenController : MonoBehaviour
//{
//    private void Update()
//    {
//        if (GameManager.Instance == null) return;
//        if (TextureCounterController.Instance == null) return;
//        if (!GameManager.Instance.IsGameActive) return; // Se o jogo acabou, não continua

//        int currentCount = TextureCounterController.Instance.TextureChangeCount;
//        int required = GameManager.Instance.requiredTextureCount;

//        if (currentCount >= required)
//        {
//            GameManager.Instance.HandleVictory(GameManager.Instance.victoryScreen);
//        }
//    }
//}
