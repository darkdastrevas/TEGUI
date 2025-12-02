using UnityEngine;
using TMPro;
using Fusion;

public class TextureCounterHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    private void Update()
    {
        // Se ainda não existe instância, não faz nada (evita erro)
        if (TextureCounterController.Instance == null)
        {
            counterText.text = "0/3";
            return;
        }

        // Lê o contador de forma segura
        int count = TextureCounterController.Instance.TextureChangeCount;

        counterText.text = count + "/3";
    }
}
