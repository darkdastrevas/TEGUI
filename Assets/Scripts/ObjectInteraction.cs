using Fusion;
using UnityEngine;

public class ObjectInteraction : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform holdPoint; // ponto onde o objeto é segurado
    [SerializeField] private float interactDist = 3f; // distância máxima para interagir
    [SerializeField] private float moveSpeed = 2.5f; // velocidade de empurrar/puxar

    private PlayerMovementDefi playerMovement;
    private CharacterController cc;
    private GameObject heldObject;
    private bool isInteracting;
    private bool axisLocked;
    private Vector3 lockedAxis;
    private Quaternion lockedRotation;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovementDefi>();
        cc = GetComponent<CharacterController>();

        if (holdPoint == null)
            Debug.LogWarning("⚠️ Campo 'holdPoint' não atribuído no Inspector!");
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

        if (isInteracting && heldObject != null)
            HandleMovement();
    }

    void TryInteract()
    {
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * interactDist, Color.yellow, 2f);

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, interactDist))
        {
            if (hit.collider.CompareTag("Interact"))
            {
                heldObject = hit.collider.gameObject;
                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;

                isInteracting = true;

                // bloqueia rotação e ativa modo de interação no player
                lockedRotation = transform.rotation;
                playerMovement.IsInteracting = true;
                playerMovement.CanRotate = false;

                Debug.Log("✅ Interagindo com objeto: " + heldObject.name);
            }
            else
            {
                Debug.Log("⚠️ Objeto atingido não tem a tag 'Interact'");
            }
        }
        else
        {
            Debug.Log("❌ Nenhum objeto atingido.");
        }
    }

    void StopInteraction()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);
            heldObject = null;
        }

        isInteracting = false;
        axisLocked = false;

        // restaura controle do player
        playerMovement.IsInteracting = false;
        playerMovement.CanRotate = true;

        Debug.Log("❎ Saiu do modo de interação.");
    }

    void HandleMovement()
    {
        // Mantém a rotação fixa
        transform.rotation = lockedRotation;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Detecta eixo inicial
        if (!axisLocked && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f))
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
                lockedAxis = Vector3.right;
            else
                lockedAxis = Vector3.forward;

            axisLocked = true;
        }

        // Define direção de movimento
        Vector3 moveDir = Vector3.zero;
        if (axisLocked)
        {
            if (lockedAxis == Vector3.right)
                moveDir = new Vector3(h, 0, 0);
            else if (lockedAxis == Vector3.forward)
                moveDir = new Vector3(0, 0, v);
        }

        // Libera eixo quando o jogador solta o input
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
            axisLocked = false;

        // Aplica movimento
        if (moveDir.sqrMagnitude > 0.01f)
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
    }
}
