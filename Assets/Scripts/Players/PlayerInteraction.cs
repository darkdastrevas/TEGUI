using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator; // Arraste o Animator do jogador aqui
    [SerializeField] private string paintTrigger = "Paint"; // Nome do trigger para a animação de pintar
    [SerializeField] private float paintDelay = 0.5f; // Delay em segundos antes de aplicar a textura (sincronizar com animação)

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private IPlayerMovement movement; // Referência automática via interface (funciona com PlayerMovementDefi ou PlayerMovement)
    private bool pressedF;

    public override void Spawned()
    {
        // Detecta automaticamente o script de movimento (PlayerMovementDefi ou PlayerMovement)
        movement = GetComponent<IPlayerMovement>();
        if (movement == null)
        {
            Debug.LogWarning("[PlayerInteraction] Nenhum script de movimento compatível encontrado (IPlayerMovement). Verifique se PlayerMovementDefi ou PlayerMovement está anexado.");
        }
    }

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
        // Verifica se o player está grounded (usando a interface)
        if (movement == null || !movement.IsGrounded)
        {
            if (debugMode) Debug.Log("[PlayerInteraction] Interação cancelada: Player não está grounded.");
            return; // Não executa a interação se não estiver no chão
        }

        Vector3 origin = transform.position + Vector3.up;
        Vector3 dir = transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactDistance, interactLayer))
        {
            var obj = hit.collider.GetComponent<ChangeTextureObject>();
            if (obj == null)
            {
                if (debugMode) Debug.Log("[PlayerInteraction] Nenhum objeto válido encontrado para interação. Animação não executada.");
                return; // Não toca animação se não houver objeto
            }

            // Toca a animação de pintar IMEDIATAMENTE (só se houver objeto)
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(paintTrigger);
                if (debugMode) Debug.Log("[PlayerInteraction] Animação de pintar tocada: " + paintTrigger);
            }

            // Inicia coroutine para aplicar a textura após o delay
            Runner.StartCoroutine(ApplyPaintDelayed(obj));
        }
        else
        {
            if (debugMode) Debug.Log("[PlayerInteraction] Raycast falhou: Nenhum objeto na direção. Animação não executada.");
        }
    }

    private System.Collections.IEnumerator ApplyPaintDelayed(ChangeTextureObject obj)
    {
        // Bloqueia movimento durante a interação
        if (movement != null)
        {
            movement.SetMovementBlocked(true);
        }

        // Aguarda o delay para sincronizar com a animação
        yield return new WaitForSeconds(paintDelay);
        if (debugMode) Debug.Log("[PlayerInteraction] Delay concluído, aplicando textura.");

        // PEGA A TAG DO GAMEOBJECT DO PLAYER
        string playerTag = gameObject.tag;

        // Escolhe a textura baseado na tag do player
        int textureIndex = 0;
        if (playerTag == "Player") textureIndex = Random.Range(1, 3); // B/B1
        else if (playerTag == "Player2") textureIndex = Random.Range(3, 5); // C/C1
        else
        {
            if (debugMode) Debug.LogWarning("[PlayerInteraction] Tag inválida: " + playerTag);
            yield break; // Sai da coroutine
        }

        if (debugMode)
            Debug.Log("[PlayerInteraction] Player " + playerTag + " selecionou textura " + textureIndex);

        // Envia RPC para todos aplicarem a textura
        obj.RpcApplyTextureIndex(textureIndex);

        // Desbloqueia movimento após a interação
        if (movement != null)
        {
            movement.SetMovementBlocked(false);
        }
    }
}
