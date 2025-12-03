using Fusion;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour, IPlayerMovement
{
    private CharacterController _controller;
    private Animator _animator;
    private LedgeGrab ledgeGrab;

    public bool IsGrounded => _controller != null && _controller.isGrounded;
    public bool IsMovementBlocked = false;

    public Vector3 _velocity;
    private bool _jumpPressed;
    private bool wasGroundedLastFrame = false;
    private bool isJumping;
    private bool isFalling;
    private float landTimer;

    private Vector3 _moveInput;

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    [Header("Lata de Tinta (Sincronizada)")]
    public GameObject paintCan;
    public float paintCanDuration = 2f;

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
                    hudScript.SetPlayer(this);
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

        if (Input.GetKeyDown(KeyCode.F))
            TryUsePaintCan();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        // NÃO bloqueia movimento com lata
        // if (PaintCanActive) return;

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

        // ==== FALLING ====
        if (!isGrounded && !isFalling && _velocity.y < -1f)
        {
            isFalling = true;
            _animator.SetBool("isFalling", true);
        }

        // ==== LANDING (sem bloquear movimento) ====
        if (isGrounded && !wasGroundedLastFrame && isFalling)
        {
            _animator.SetTrigger("Land");
            _animator.SetBool("isFalling", false);
            _animator.SetBool("isJumping", false);

            isFalling = false;
            isJumping = false;
        }

        // ==== JUMP ====
        if (_jumpPressed && isGrounded)
        {
            _velocity.y = JumpForce;
            _animator.SetBool("isJumping", true);
            isJumping = true;
        }

        // ==== GRAVITY ====
        if (isGrounded && _velocity.y < 0)
            _velocity.y = -1f;
        else
            _velocity.y += GravityValue * Runner.DeltaTime;

        // ==== MOVEMENT (NUNCA BLOQUEADO) ====
        Vector3 move = _moveInput.normalized * PlayerSpeed * Runner.DeltaTime;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        // ==== ANIMATIONS ====
        if (move.sqrMagnitude > 0.001f)
        {
            transform.forward = move.normalized;
            _animator.SetBool("isWalking", true);
            _animator.SetBool("isIdle", false);
        }
        else if (isGrounded)
        {
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isIdle", true);
        }

        _jumpPressed = false;
        wasGroundedLastFrame = isGrounded;
    }

    // ===========================
    //     PAINT CAN (RPC)
    // ===========================

    private void TryUsePaintCan()
    {
        if (!_controller.isGrounded) return;
        if (PaintCanActive) return;

        RPC_UsePaintCan();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_UsePaintCan()
    {
        if (PaintCanActive) return;

        PaintCanActive = true;
        StartCoroutine(PaintCanRoutine());
    }

    private IEnumerator PaintCanRoutine()
    {
        if (paintCan != null)
            paintCan.SetActive(true);

        yield return new WaitForSeconds(paintCanDuration);

        PaintCanActive = false;
    }

    public override void Render()
    {
        if (PaintCanActive != lastPaintCanState)
        {
            lastPaintCanState = PaintCanActive;

            if (paintCan != null)
                paintCan.SetActive(PaintCanActive);
        }
    }

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
