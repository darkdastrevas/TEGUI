using Fusion;
using UnityEngine;

public class PushableObject : NetworkBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 3f;
    public float boxRadius = 0.45f;
    public LayerMask collisionMask;

    private Rigidbody rb;
    private PushableNetworkController netController;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        netController = GetComponent<PushableNetworkController>();
    }

    public bool CanMove(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;

        return !Physics.Raycast(
            origin,
            dir,
            boxRadius,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void Move(Vector3 direction)
    {
        if (netController.IsBeingCarried)
            return;

        if (!CanMove(direction))
            return;

        Vector3 targetPos = rb.position + direction * moveSpeed * Runner.DeltaTime;
        rb.MovePosition(targetPos);
    }
}
