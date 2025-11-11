using Fusion;
using UnityEngine;

public class ObjectInteraction : NetworkBehaviour
{
    [Header("Interação")]
    [SerializeField] private float interactDist = 3f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float carrySpeedMultiplier = 0.6f;

    [Header("Refs")]
    [SerializeField] private Animator animator;

    private PlayerMovementDefi playerMovement;
    private CharacterController cc;

    private NetworkObject heldNetObject;
    private bool isInteracting;
    private bool axisLocked;
    private Quaternion lockedRotation;
    private Vector3 lockedAxis;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovementDefi>();
        cc = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting)
                TryInteract();
            else
                StopInteraction();
        }

        if (isInteracting && heldNetObject != null)
            HandleMovement();
    }

    void TryInteract()
    {
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out var hit, interactDist))
            return;

        if (!hit.collider.CompareTag("Interact"))
            return;

        var netObj = hit.collider.GetComponent<NetworkObject>();
        if (netObj == null) return;

        heldNetObject = netObj;

        RPC_RequestStartCarry(heldNetObject.Id, Object.Id);

        isInteracting = true;
        lockedRotation = transform.rotation;

        playerMovement.IsInteracting = true;
        playerMovement.CanRotate = false;
        playerMovement.PlayerSpeed *= carrySpeedMultiplier;

        if (animator != null)
        {
            animator.SetBool("isPushing", true);
            animator.SetBool("PushingIdle", true);
        }
    }

    void StopInteraction()
    {
        if (heldNetObject != null)
        {
            RPC_RequestStopCarry(heldNetObject.Id);
            heldNetObject = null;
        }

        isInteracting = false;
        axisLocked = false;

        playerMovement.IsInteracting = false;
        playerMovement.CanRotate = true;
        playerMovement.PlayerSpeed /= carrySpeedMultiplier;

        ResetPushAnimations();
        if (animator != null)
        {
            animator.SetBool("PushingIdle", false);
            animator.SetBool("isPushing", false);
        }
    }

    void HandleMovement()
    {
        transform.rotation = lockedRotation;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Detecta input e lock eixo
        if (!axisLocked && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f))
        {
            lockedAxis = Mathf.Abs(h) > Mathf.Abs(v) ? Vector3.right : Vector3.forward;
            axisLocked = true;
        }

        Vector3 moveDir = axisLocked
            ? (lockedAxis == Vector3.right ? new Vector3(h, 0, 0) : new Vector3(0, 0, v))
            : Vector3.zero;

        if (moveDir.sqrMagnitude < 0.01f)
        {
            ResetPushAnimations();
            if (animator != null) animator.SetBool("PushingIdle", true);
            axisLocked = false;
            return;
        }

        // Sempre ativa animações baseadas no input (mesmo se não puder mover)
        UpdatePushAnimations(moveDir);

        var pushable = heldNetObject.GetComponent<PushableObject>();
        if (pushable == null || !pushable.CanMove(moveDir.normalized))
        {
            // Não move o player/objeto se obstáculo, mas animações já ativadas
            return;
        }

        // Só move se puder
        cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        RPC_MoveObject(heldNetObject.Id, moveDir.normalized);
    }

    void UpdatePushAnimations(Vector3 moveDir)
    {
        if (animator == null) return;

        Vector3 localMove = transform.InverseTransformDirection(moveDir);

        bool forward = localMove.z > 0.5f;
        bool backward = localMove.z < -0.5f;
        bool right = localMove.x > 0.5f;
        bool left = localMove.x < -0.5f;

        animator.SetBool("PushingIdle", false);
        animator.SetBool("PushForward", forward);
        animator.SetBool("PushBackward", backward);
        animator.SetBool("PushRight", right);
        animator.SetBool("PushLeft", left);
    }

    void ResetPushAnimations()
    {
        if (animator == null) return;

        animator.SetBool("PushForward", false);
        animator.SetBool("PushBackward", false);
        animator.SetBool("PushRight", false);
        animator.SetBool("PushLeft", false);
    }

    // RPCs
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStartCarry(NetworkId objectId, NetworkId playerId)
    {
        var obj = Runner.FindObject(objectId);
        obj?.GetComponent<PushableNetworkController>()?.StartCarrying(playerId);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_MoveObject(NetworkId objectId, Vector3 dir)
    {
        var obj = Runner.FindObject(objectId);
        obj?.GetComponent<PushableObject>()?.Move(dir);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStopCarry(NetworkId objectId)
    {
        var obj = Runner.FindObject(objectId);
        obj?.GetComponent<PushableNetworkController>()?.StopCarrying();
    }
}
