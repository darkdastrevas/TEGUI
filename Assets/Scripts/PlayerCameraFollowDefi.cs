using Fusion;
using UnityEngine;

public class PlayerCameraFollowDefi : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 3f, -6f);
    [SerializeField] private Vector3 fixedRotation = new Vector3(20f, 0f, 0f);

    private Transform target;
    private bool targetLocked = false;

    private void LateUpdate()
    {
        if (!targetLocked)
        {
            TryFindLocalPlayer();
            return;
        }

        if (target == null)
            return;

        // Remove o parenting se estiver definido (para evitar herança de movimento)
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Define a posição absoluta no mundo (relativa ao player, mas fixa)
        transform.position = target.position + offset;

        // Mantém a rotação fixa no espaço mundial
        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    private void TryFindLocalPlayer()
    {
        var players = FindObjectsOfType<NetworkObject>();

        foreach (var p in players)
        {
            if (p.HasInputAuthority && p.GetComponent<PlayerMovementDefi>() != null)
            {
                target = p.transform;
                targetLocked = true;
                Debug.Log($"[CameraFollow] Vinculada ao player local: {p.name}");
                break;
            }
        }
    }
}
