using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PushableNetworkController : NetworkBehaviour
{
    [Header("Configurações de Carry")]
    [SerializeField] private Vector3 localCarryOffset = new Vector3(0, 0, 1f);
    [SerializeField] private bool disableRigidbodyDuringCarry = true;

    private NetworkId carryingPlayerId;
    private bool isBeingCarried;
    private Rigidbody rb;
    private Collider col;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void StartCarrying(NetworkId playerId)
    {
        if (!Object.HasStateAuthority) return;

        carryingPlayerId = playerId;
        isBeingCarried = true;

        var playerObj = Runner.FindObject(carryingPlayerId);
        if (playerObj == null) return;

        // desativa colisão
        if (col != null)
            col.enabled = false;

        // Rigidbody kinematic
        if (rb != null)
            rb.isKinematic = true;

        // seta parent mantendo posição
        transform.SetParent(playerObj.transform, true);

        // aplica offset correto
        transform.localPosition = localCarryOffset;
        transform.localRotation = Quaternion.identity;
    }

    public void StopCarrying()
    {
        if (!Object.HasStateAuthority) return;

        carryingPlayerId = default;
        isBeingCarried = false;

        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;

        transform.SetParent(null, true);

        transform.position = worldPos;
        transform.rotation = worldRot;

        // reativa colisão
        if (col != null)
            col.enabled = true;

        // reativa física
        if (rb != null)
            rb.isKinematic = false;
    }
}
