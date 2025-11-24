using Fusion;
using TMPro;
using UnityEngine;

public class CountdownTimer : NetworkBehaviour
{
    [Header("Config")]
    public float startTime = 10f;

    [Networked]
    public float CurrentTime { get; set; }

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private bool uiReady = false;

    public override void Spawned()
    {
        // Inicializa somente no State Authority
        if (Object.HasStateAuthority)
            CurrentTime = startTime;

        uiReady = true;
        UpdateUI();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (CurrentTime <= 0) return;

        CurrentTime -= Runner.DeltaTime;

        if (CurrentTime <= 0)
        {
            CurrentTime = 0;

            // Chamar o GameManager para notificar o fim.
            // O State Authority dispara a ação no servidor.
            GameManager.Instance.OnTimeExpired();

            Debug.Log("O TEMPO ACABOU (STATE AUTHORITY)!");
        }
    }

    private void Update()
    {
        if (!uiReady) return;   // impede acesso antes do Spawned
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText == null) return;

        float t = CurrentTime;

        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);

        timerText.text = $"{m:0}:{s:00}";
    }
}
