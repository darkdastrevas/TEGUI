using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    public BridgeButton button;

    private bool playerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("[BUTTON] Player entrou no botão.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("[BUTTON] Player saiu do botão.");
        }
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[BUTTON] F pressionado dentro do botão → Tentando usar.");
            button.TryUse();
        }
    }
}
