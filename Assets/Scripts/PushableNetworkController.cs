using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PushableNetworkController : NetworkBehaviour
{
    [Header("Carregar")]
    [SerializeField] private Vector3 localCarryOffset = new Vector3(0, 0.9f, 1.0f);

    private NetworkId carryingPlayerId;
    private bool isBeingCarried;

    private Rigidbody rb;
    private Collider col;

    public bool IsBeingCarried => isBeingCarried;

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

        if (rb != null)
            rb.isKinematic = true;

        if (col != null)
            col.enabled = false;

        transform.SetParent(playerObj.transform, true);

        transform.localPosition = localCarryOffset;
        transform.localRotation = Quaternion.identity;
    }

    public void StopCarrying()
    {
        if (!Object.HasStateAuthority) return;

        carryingPlayerId = default;
        isBeingCarried = false;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        transform.SetParent(null, true);

        transform.position = pos;
        transform.rotation = rot;

        if (col != null)
            col.enabled = true;

        if (rb != null)
            rb.isKinematic = false;
    }
}
