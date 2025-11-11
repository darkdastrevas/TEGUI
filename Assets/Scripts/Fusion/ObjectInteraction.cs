using Fusion;
using UnityEngine;

public class ObjectInteraction : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform holdPoint; // ponto onde o objeto é segurado
    [SerializeField] private float interactDist = 3f; // distância máxima para interagir
    [SerializeField] private float moveSpeed = 2.5f; // velocidade de empurrar/puxar
    [SerializeField] private Animator animator; // 🎬 referência ao Animator do player

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

                // ativa estado base de interação
                animator.SetBool("isPushing", true);
                animator.SetBool("PushingIdle", true);

                Debug.Log("Interagindo com objeto: " + heldObject.name);
            }
            else
            {
                Debug.Log("Objeto atingido não tem a tag 'Interact'");
            }
        }
        else
        {
            Debug.Log("Nenhum objeto atingido.");
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

        // 🎬 desativa todas as animações de push
        animator.SetBool("isPushing", false);
        animator.SetBool("PushForward", false);
        animator.SetBool("PushBackward", false);
        animator.SetBool("PushRight", false);
        animator.SetBool("PushLeft", false);
        animator.SetBool("PushingIdle", false);

        Debug.Log("Saiu do modo de interação.");
    }

    void HandleMovement()
    {
        // mantem a rotação fixa
        transform.rotation = lockedRotation;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // detecta eixo inicial
        if (!axisLocked && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f))
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
                lockedAxis = Vector3.right;
            else
                lockedAxis = Vector3.forward;

            axisLocked = true;
        }

        // define direção de movimento
        Vector3 moveDir = Vector3.zero;
        if (axisLocked)
        {
            if (lockedAxis == Vector3.right)
                moveDir = new Vector3(h, 0, 0);
            else if (lockedAxis == Vector3.forward)
                moveDir = new Vector3(0, 0, v);
        }

        // libera eixo quando o jogador solta o input
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            axisLocked = false;
            ResetPushAnimations();
            animator.SetBool("PushingIdle", true); // perdi ela vei :(
        }

        // Aplica movimento
        if (moveDir.sqrMagnitude > 0.01f)
        {
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            UpdatePushAnimations(moveDir);
        }
        else
        {
            ResetPushAnimations();
            animator.SetBool("PushingIdle", true); // perdi essa animacao :(
        }
    }

    // atualiza animações conforme direção do movimento
    void UpdatePushAnimations(Vector3 moveDir)
    {
        bool forward = Vector3.Dot(moveDir, transform.forward) > 0.5f;
        bool backward = Vector3.Dot(moveDir, -transform.forward) > 0.5f;
        bool right = Vector3.Dot(moveDir, transform.right) > 0.5f;
        bool left = Vector3.Dot(moveDir, -transform.right) > 0.5f;

        animator.SetBool("PushingIdle", false); // 

        animator.SetBool("PushForward", forward);
        animator.SetBool("PushBackward", backward);
        animator.SetBool("PushRight", right);
        animator.SetBool("PushLeft", left);
    }

    // reset das animações
    void ResetPushAnimations()
    {
        animator.SetBool("PushForward", false);
        animator.SetBool("PushBackward", false);
        animator.SetBool("PushRight", false);
        animator.SetBool("PushLeft", false);
    }
}