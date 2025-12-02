using UnityEngine;
using Fusion;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class LedgeGrab : NetworkBehaviour
{
    // ───────────────────────────────────────────────────────────────────────────────
    #region === REFERENCES ===
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private Animator animator;
    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === SETTINGS ===
    [Header("Ledge Config")]
    public LayerMask ledgeLayer;
    public float rayLength = 0.8f;
    public float rayHeight = 1.5f;
    public float grabHeightOffset = 0.1f;
    public float grabForwardOffset = 0.25f;
    public float moveSpeed = 2f;
    public int rayAmount = 5;
    public float rayOffset = 0.15f;

    // NOVO: Prefab da Partícula
    [Header("Efeitos")]
    public GameObject grabEffectPrefab;
    public float effectDuration = 1.0f;
    public float effectForwardOffset = 0.1f;

    [Header("Lateral Rays")]
    public float lateralRayLength = 0.5f;
    public float lateralRayOffset = 0.1f;
    public float ledgeContinuityLateralOffset = 0.25f;

    [Header("Climb")]
    public float climbUpHeight = 1.5f;
    public float climbUpSpeed = 3f;

    [Header("Input & Behavior")]
    public bool autoGrab = true;
    public KeyCode releaseKey = KeyCode.LeftShift;

    [Header("Delay do Jump (para sincronizar animação)")]
    public float jumpDelay = 0.25f;
    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === CONSTANTS ===
    private const float grabCooldown = 0.45f;
    private const float grabJumpInputDelay = 0.3f;
    private const float jumpCooldownDuration = 0.5f; // Delay para impedir spamming de grab jumps
    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === STATE ===
    public bool isGrabbing;
    private bool canGrab;
    private bool grabBlocked;
    private bool jumpBlockedAfterGrab;
    private bool jumpOnCooldown; // Controla cooldown após grab jump
    private bool isGrabJumping; // Novo: impede múltiplos grab jumps simultâneos
    public bool isClimbing;
    private bool jumpLockedUntilEnd;

    private RaycastHit lastLedgeHit;
    private Quaternion grabRotation;

    // Inputs
    private float horizontalInput;
    private bool jumpPressed;
    private bool releasePressed;


    // Debug
    private Vector3 debugRayOrigin;
    private Vector3 debugRayHitPoint;
    private bool debugCanGrab;
    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === SPAWN ===

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === UPDATE (PROS INPUTS) ===

    private void Update()
    {
        if (!HasInputAuthority)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Verifica jumpOnCooldown e jumpBlockedAfterGrab antes de permitir jumpPressed
        if (!jumpOnCooldown && !jumpBlockedAfterGrab && Input.GetButtonDown("Jump"))
            jumpPressed = true;

        if (Input.GetKeyDown(releaseKey))
            releasePressed = true;

        DrawGrabDebugRays();
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === FIXED UPDATE ===

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        TryDetectLedge();

        if (isGrabbing)
        {
            HandleHorizontalMovement();

            // Modificado: Adiciona !isGrabJumping para impedir múltiplos grab jumps
            if (jumpPressed && !isGrabJumping)
            {
                animator?.SetTrigger("GrabJump");
                animator?.SetBool("IsGrabIdle", false);
                Runner.StartCoroutine(JumpDelayRoutine());
            }

            if (releasePressed)
                ReleaseLedge();
        }

        jumpPressed = false;
        releasePressed = false;
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === DETECTOR ===

    private void TryDetectLedge()
    {
        if (isGrabbing || controller.isGrounded || grabBlocked)
            return;

        for (int i = 0; i < rayAmount; i++)
        {
            Vector3 origin = transform.position + Vector3.up * (rayHeight + rayOffset * i);

            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, rayLength, ledgeLayer) &&
                hit.collider.CompareTag("Ledge"))
            {
                lastLedgeHit = hit;
                canGrab = true;

                if (autoGrab)
                    StartGrab();

                return;
            }
        }

        canGrab = false;
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === AGARRAR/SOLTAR ===

    private void StartGrab()
    {
        isGrabbing = true;
        canGrab = false;

        controller.enabled = false;
        playerMovement.enabled = false;

        grabRotation = Quaternion.LookRotation(-lastLedgeHit.normal, Vector3.up);
        transform.rotation = grabRotation;

        Vector3 grabPos =
            lastLedgeHit.point +
            Vector3.up * grabHeightOffset -
            grabRotation * Vector3.forward * grabForwardOffset;

        transform.position = grabPos;

        animator?.SetTrigger("Grab");
        animator?.SetBool("IsGrabbing", true);
        animator?.SetBool("IsGrabIdle", true);

        Runner.StartCoroutine(GrabInputDelay());

        // NOVO: Chama o RPC para sincronizar o efeito de partícula
        RPC_PlayGrabEffect(lastLedgeHit.point, grabRotation, effectForwardOffset);
    }

    private void ReleaseLedge()
    {
        jumpLockedUntilEnd = false;
        isGrabbing = false;

        controller.enabled = true;
        playerMovement.enabled = true;

        animator?.SetBool("IsGrabbing", false);
        animator?.SetBool("IsGrabWalkingLeft", false);
        animator?.SetBool("IsGrabWalkingRight", false);
        animator?.SetBool("IsGrabIdle", false);

        transform.position -= transform.forward * 0.1f;

        Runner.StartCoroutine(GrabCooldownRoutine());
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === EFEITOS (RPC) ===

    // RPC: Chamado pelo InputAuthority e executado em todos os clientes
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayGrabEffect(Vector3 hitPoint, Quaternion playerRotation, float forwardOffset)
    {
        // O playerRotation já está alinhado com a parede (forward do player aponta para a parede).
        // Usamos playerRotation * Vector3.forward para obter a direção PARA A PAREDE.
        // O hitPoint é o ponto na superfície da parede.

        // CÁLCULO DE POSIÇÃO AJUSTADO:
        // Ponto de Acerto - (Direção PARA a parede * Offset)
        Vector3 spawnPosition = hitPoint - (playerRotation * Vector3.forward * forwardOffset);

        // Rotação: Vira a partícula para longe da parede (normal do hit)
        // OBS: Você pode querer usar Quaternion.identity ou a rotação do player (playerRotation) 
        // dependendo de como sua partícula está configurada. Usaremos a normal para apontar 
        // "para fora" da parede, como estava na versão anterior, mas com o ponto ajustado.

        // A 'lastLedgeHit' não está disponível no RPC, então calcularemos a rotação
        // de forma simples ou, se for um efeito geral, podemos usar a rotação do player.
        // Vamos usar uma rotação simples para a partícula.
        Quaternion effectRotation = Quaternion.identity;

        // Instancia o efeito
        if (grabEffectPrefab != null)
        {
            GameObject effect = Instantiate(grabEffectPrefab, spawnPosition, effectRotation);

            // Destrói a partícula após a duração definida
            Destroy(effect, effectDuration);
        }
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === MOVIMENTO AGARRANDO ===

    private void HandleHorizontalMovement()
    {
        transform.rotation = grabRotation;

        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            animator?.SetBool("IsGrabIdle", true);
            animator?.SetBool("IsGrabWalkingLeft", false);
            animator?.SetBool("IsGrabWalkingRight", false);
            return;
        }

        Vector3 dir = horizontalInput > 0 ? transform.right : -transform.right;

        if (BlockedByWall(dir))
            return;

        if (!HasLedgeAhead(dir))
            return;

        transform.position += dir * moveSpeed * Runner.DeltaTime;

        animator?.SetBool("IsGrabIdle", false);
        animator?.SetBool("IsGrabWalkingLeft", horizontalInput < 0);
        animator?.SetBool("IsGrabWalkingRight", horizontalInput > 0);
    }

    private bool BlockedByWall(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * lateralRayOffset;
        return Physics.Raycast(origin, dir, out RaycastHit hit, lateralRayLength) &&
                !hit.collider.CompareTag("Ledge");
    }

    private bool HasLedgeAhead(Vector3 dir)
    {
        Vector3 origin =
            transform.position +
            dir * ledgeContinuityLateralOffset +
            Vector3.up * lateralRayOffset;

        return Physics.Raycast(
            origin,
            transform.forward,
            out RaycastHit hit,
            rayLength * 0.8f,
            ledgeLayer
        );
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === SUBIR ===

    private System.Collections.IEnumerator JumpDelayRoutine()
    {
        isGrabJumping = true; // Novo: Bloqueia novos grab jumps até terminar

        yield return new WaitForSeconds(jumpDelay);

        if (!isGrabbing)
            yield break;

        // Ativa cooldown após grab jump para impedir spamming
        jumpOnCooldown = true;
        Runner.StartCoroutine(JumpCooldownRoutine());

        Runner.StartCoroutine(ClimbUpRoutine());
    }

    private System.Collections.IEnumerator ClimbUpRoutine()
    {
        isClimbing = true;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * climbUpHeight;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * climbUpSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        controller.enabled = true;
        playerMovement.enabled = true;

        animator.SetBool("IsGrabbing", false);
        animator.SetBool("IsGrabIdle", false);

        isClimbing = false;
        isGrabbing = false;
        jumpLockedUntilEnd = false;

        playerMovement._velocity.y = 0f;

        isGrabJumping = false; // Novo: Libera para permitir novos grab jumps
    }


    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === TIMERS ===

    private IEnumerator GrabCooldownRoutine()
    {
        grabBlocked = true;
        yield return new WaitForSeconds(grabCooldown);
        grabBlocked = false;
    }

    private IEnumerator GrabInputDelay()
    {
        jumpBlockedAfterGrab = true;
        yield return new WaitForSeconds(grabJumpInputDelay);
        jumpBlockedAfterGrab = false;
    }

    // Coroutine para resetar o cooldown de jump após grab jump
    private IEnumerator JumpCooldownRoutine()
    {
        yield return new WaitForSeconds(jumpCooldownDuration);
        jumpOnCooldown = false;
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────


    // ───────────────────────────────────────────────────────────────────────────────
    #region === DEBUG ===

    private void DrawGrabDebugRays()
    {
        if (controller.isGrounded) { debugCanGrab = false; return; }

        for (int i = 0; i < rayAmount; i++)
        {
            Vector3 origin = transform.position + Vector3.up * (rayHeight + rayOffset * i);

            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, rayLength, ledgeLayer))
            {
                debugCanGrab = true;
                debugRayOrigin = origin;
                debugRayHitPoint = hit.point;
                Debug.DrawLine(origin, hit.point, Color.green);
                return;
            }

            Debug.DrawLine(origin, origin + transform.forward * rayLength, Color.red);
        }
    }

    #endregion
    // ───────────────────────────────────────────────────────────────────────────────
}