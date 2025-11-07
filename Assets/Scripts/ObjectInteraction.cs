using Fusion;
using UnityEngine;

public class ObjectInteraction : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float interactDist = 3f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float carrySpeedMultiplier = 0.6f;
    [SerializeField] private Animator animator;

    private PlayerMovementDefi playerMovement;
    private CharacterController cc;

    private NetworkObject heldNetObject;
    private bool isInteracting;
    private bool axisLocked;
    private Vector3 lockedAxis;
    private Quaternion lockedRotation;

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
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, interactDist))
        {
            Debug.Log("Nada para interagir.");
            return;
        }

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

        Debug.Log("Interagiu com " + heldNetObject.name);
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
            animator.SetBool("PushingIdle", false);

        Debug.Log("StopInteraction enviado.");
    }

    void HandleMovement()
    {
        transform.rotation = lockedRotation;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (!axisLocked && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f))
        {
            lockedAxis = Mathf.Abs(h) > Mathf.Abs(v) ? Vector3.right : Vector3.forward;
            axisLocked = true;
        }

        Vector3 moveDir = Vector3.zero;

        if (axisLocked)
        {
            moveDir = (lockedAxis == Vector3.right)
                ? new Vector3(h, 0, 0)
                : new Vector3(0, 0, v);
        }

        if (moveDir.sqrMagnitude > 0.01f && heldNetObject != null)
        {
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            RPC_MoveObject(heldNetObject.Id, moveDir.normalized);
            UpdatePushAnimations(moveDir);
        }
        else
        {
            ResetPushAnimations();
            if (animator != null)
                animator.SetBool("PushingIdle", true);

            axisLocked = false;
        }
    }

    void UpdatePushAnimations(Vector3 moveDir)
    {
        if (animator == null) return;

        bool forward = Vector3.Dot(moveDir, transform.forward) > 0.5f;
        bool backward = Vector3.Dot(moveDir, -transform.forward) > 0.5f;
        bool right = Vector3.Dot(moveDir, transform.right) > 0.5f;
        bool left = Vector3.Dot(moveDir, -transform.right) > 0.5f;

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

    // ---------------- RPCs ---------------- //

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStartCarry(NetworkId objectId, NetworkId playerId)
    {
        var obj = Runner.FindObject(objectId);
        if (obj == null) return;

        var ctrl = obj.GetComponent<PushableNetworkController>();
        if (ctrl != null)
            ctrl.StartCarrying(playerId);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_MoveObject(NetworkId objectId, Vector3 direction)
    {
        var obj = Runner.FindObject(objectId);
        if (obj == null) return;

        var pushable = obj.GetComponent<PushableObject>();
        if (pushable != null)
            pushable.Move(direction);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStopCarry(NetworkId objectId)
    {
        var obj = Runner.FindObject(objectId);
        if (obj == null) return;

        var ctrl = obj.GetComponent<PushableNetworkController>();
        if (ctrl != null)
            ctrl.StopCarrying();
    }
}
