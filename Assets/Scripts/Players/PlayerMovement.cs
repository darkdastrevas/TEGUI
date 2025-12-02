using Fusion;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour, IPlayerMovement
{
    private CharacterController _controller;
    private Animator _animator;
    private LedgeGrab ledgeGrab;

    // Interface
    public bool IsGrounded => _controller != null && _controller.isGrounded;

    public bool IsMovementBlocked = false;

    public Vector3 _velocity;
    private bool _jumpPressed;
    private bool wasGroundedLastFrame = false;
    private bool isJumping;
    private bool isFalling;
    private bool isLanding;
    private float landTimer;

    private Vector3 _moveInput;

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;
    public float landLockTime = 0.4f;

    [Header("Lata de Tinta (Sincronizada)")]
    public GameObject paintCan;
    public float paintCanDuration = 2f;

    // Estado sincronizado da lata
    [Networked] private bool PaintCanActive { get; set; }
    private bool lastPaintCanState = false;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        ledgeGrab = GetComponent<LedgeGrab>();

        if (paintCan != null)
            paintCan.SetActive(false);

        if (Object.HasInputAuthority)
        {
            var hudObject = GameObject.Find("PlayerHUD");

            if (hudObject != null)
            {
                var hudScript = hudObject.GetComponent<PlayerHUD>();

                if (hudScript != null)
                {
                    hudScript.SetPlayer(this);
                    Debug.Log("[PlayerMovement] HUD configurado.");
                }
            }
        }
    }

    void Update()
    {
        if (!HasInputAuthority)
            return;

        _moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;

        // Entrada da lata
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryUsePaintCan();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        if (PaintCanActive)
        {
            // Não movimenta enquanto a lata está ativa
            return;
        }

        // Impede movimento ao escalar
        if (ledgeGrab != null && (ledgeGrab.isGrabbing || ledgeGrab.isClimbing))
        {
            _velocity.y = -1f;
            _jumpPressed = false;
            return;
        }

        if (IsMovementBlocked)
        {
            _velocity = Vector3.zero;
            return;
        }

        bool isGrounded = _controller.isGrounded;

        // ==== QUEDA ====
        if (!isGrounded && !isFalling && _velocity.y < -1f)
        {
            isFalling = true;
            _animator.SetBool("isFalling", true);
        }

        // ==== ATERRISSAGEM ====
        if (isGrounded && !wasGroundedLastFrame && isFalling)
        {
            _animator.SetTrigger("Land");
            _animator.SetBool("isFalling", false);
            _animator.SetBool("isJumping", false);

            isLanding = true;
            isFalling = false;
            isJumping = false;
            landTimer = 0;
        }

        if (isLanding)
        {
            landTimer += Runner.DeltaTime;
            if (landTimer >= landLockTime)
            {
                isLanding = false;
                _animator.ResetTrigger("Land");
            }
        }

        // ==== PULO ====
        if (_jumpPressed && isGrounded && !isLanding)
        {
            _velocity.y = JumpForce;
            _animator.SetBool("isJumping", true);
            isJumping = true;
        }

        // ==== GRAVIDADE ====
        if (isGrounded && _velocity.y < 0)
            _velocity.y = -1f;
        else
            _velocity.y += GravityValue * Runner.DeltaTime;

        // ==== MOVIMENTO ====
        Vector3 move = Vector3.zero;

        if (!isLanding)
            move = _moveInput.normalized * PlayerSpeed * Runner.DeltaTime;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        // ==== ANIMAÇÕES ====
        if (move.sqrMagnitude > 0.001f && !isLanding)
        {
            transform.forward = move.normalized;
            _animator.SetBool("isWalking", true);
            _animator.SetBool("isIdle", false);
        }
        else if (!isLanding && isGrounded)
        {
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isIdle", true);
        }

        _jumpPressed = false;
        wasGroundedLastFrame = isGrounded;
    }

    // ===============================
    //      SISTEMA DA LATA (RPC)
    // ===============================

    private void TryUsePaintCan()
    {
        if (!_controller.isGrounded) return;

        if (PaintCanActive) return;

        // CHAMA O RPC (sincroniza a todos)
        RPC_UsePaintCan();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_UsePaintCan()
    {
        if (PaintCanActive) return;

        // Ativa sincronizado
        PaintCanActive = true;

        // Inicia coroutine somente no StateAuthority
        StartCoroutine(PaintCanRoutine());
    }

    private IEnumerator PaintCanRoutine()
    {
        // Ativar objeto localmente no host
        if (paintCan != null)
            paintCan.SetActive(true);

        yield return new WaitForSeconds(paintCanDuration);

        // Desativar sincronizado
        PaintCanActive = false;
    }

    public override void Render()
    {
        // Atualiza estado visual nos clientes
        if (PaintCanActive != lastPaintCanState)
        {
            lastPaintCanState = PaintCanActive;

            if (paintCan != null)
                paintCan.SetActive(PaintCanActive);
        }
    }

    // Interface
    public void SetMovementBlocked(bool isBlocked)
    {
        IsMovementBlocked = isBlocked;

        if (isBlocked && _animator != null)
        {
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isIdle", true);
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isFalling", false);
        }
    }
}
