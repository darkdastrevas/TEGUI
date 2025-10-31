using UnityEngine;
using Fusion;

public class ObjectInteraction : NetworkBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private Transform holdPoint; // ponto onde o objeto ficará preso
    [SerializeField] private float moveSpeed = 2.5f;

    private GameObject heldObject;
    private CharacterController cc;

    private bool axisLocked;
    private Vector3 lockedAxis; // X ou Z
    private bool isHolding;

    private PlayerMovementDefi playerMovement; // referência para bloquear rotação

    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovementDefi>();
    }

    void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHolding)
                TryPickup();
            else
                DropObject();
        }

        if (isHolding)
            HandleMovement();
    }

    void TryPickup()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            GameObject target = hit.collider.gameObject;

            // Verifica se tem tag "Interact"
            if (target.CompareTag("Interact"))
            {
                heldObject = target;
                heldObject.transform.SetParent(holdPoint != null ? holdPoint : transform);
                heldObject.transform.localPosition = Vector3.zero;

                if (heldObject.TryGetComponent<Rigidbody>(out var rb))
                    rb.isKinematic = true;

                axisLocked = false;
                isHolding = true;

                // Faz o player olhar pro objeto
                Vector3 dir = (heldObject.transform.position - transform.position).normalized;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir);

                // ✅ Bloqueia rotação do player enquanto segura
                if (playerMovement != null)
                    playerMovement.CanRotate = false;

                Debug.Log($"Pegou o objeto: {heldObject.name}");
            }
            else
            {
                Debug.Log("O objeto atingido não tem a tag Interact.");
            }
        }
    }

    void DropObject()
    {
        if (heldObject == null)
            return;

        heldObject.transform.SetParent(null);

        if (heldObject.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        Debug.Log($"Soltou o objeto: {heldObject.name}");
        heldObject = null;
        axisLocked = false;
        isHolding = false;

        // ✅ Libera rotação do player ao soltar
        if (playerMovement != null)
            playerMovement.CanRotate = true;
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v);

        // Define o eixo no primeiro movimento
        if (inputDir.magnitude > 0.1f && !axisLocked)
        {
            if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.z))
                lockedAxis = Vector3.right * Mathf.Sign(inputDir.x);
            else
                lockedAxis = Vector3.forward * Mathf.Sign(inputDir.z);

            axisLocked = true;
        }

        // Move apenas no eixo travado
        Vector3 moveDir = Vector3.zero;
        if (axisLocked)
            moveDir = lockedAxis * (Mathf.Abs(h) + Mathf.Abs(v));

        if (moveDir.magnitude > 0.1f)
        {
            cc.Move(moveDir * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(lockedAxis);
        }
    }
}
