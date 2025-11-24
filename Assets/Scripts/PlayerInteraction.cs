using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool pressedF;

    void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.F))
        {
            pressedF = true;
            if (debugMode) Debug.Log("[PlayerInteraction] F apertado");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (pressedF)
        {
            TryInteract();
            pressedF = false;
        }
    }

    private void TryInteract()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 dir = transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactDistance, interactLayer))
        {
            var obj = hit.collider.GetComponent<ChangeTextureObject>();
            if (!obj) return;

            // PEGA A TAG DO GAMEOBJECT DO PLAYER
            string playerTag = gameObject.tag;

            // Escolhe a textura baseado na tag do player
            int textureIndex = 0;
            if (playerTag == "Player") textureIndex = Random.Range(1, 3); // B/B1
            else if (playerTag == "Player2") textureIndex = Random.Range(3, 5); // C/C1
            else
            {
                if (debugMode) Debug.LogWarning("[PlayerInteraction] Tag inválida: " + playerTag);
                return;
            }

            if (debugMode)
                Debug.Log("[PlayerInteraction] Player " + playerTag + " selecionou textura " + textureIndex);

            // Envia RPC para todos aplicarem a textura
            obj.RpcApplyTextureIndex(textureIndex);
        }
    }
}
