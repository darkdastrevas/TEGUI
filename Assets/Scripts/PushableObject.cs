using Fusion;
using UnityEngine;

public class PushableObject : NetworkBehaviour
{
    [Header("Configurações")]
    public float moveSpeed = 3f;
    public float smoothness = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 direction)
    {
        Vector3 targetPos = rb.position + direction * moveSpeed * Runner.DeltaTime;
        rb.MovePosition(Vector3.Lerp(rb.position, targetPos, smoothness * Runner.DeltaTime));
    }
}
