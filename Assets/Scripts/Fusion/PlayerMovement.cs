using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    private Animator _animator;
    private LedgeGrab ledgeGrab;

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

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        ledgeGrab = GetComponent<LedgeGrab>();
    }

    void Update()
    {
        if (!HasInputAuthority)
            return;

        _moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        // BLOQUEIO TOTAL DURANTE ESCALADA
        if (ledgeGrab != null && (ledgeGrab.isGrabbing | ledgeGrab.isClimbing))
        {
            _velocity.y = -1f;
            _jumpPressed = false;
            return;
        }

        bool isGrounded = _controller.isGrounded;

        // Início da queda
        if (!isGrounded && !isFalling && _velocity.y < -1f)
        {
            isFalling = true;
            _animator.SetBool("isFalling", true);
        }

        // Aterrissagem
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

        // Timer do land
        if (isLanding)
        {
            landTimer += Runner.DeltaTime;
            if (landTimer >= landLockTime)
            {
                isLanding = false;
                _animator.ResetTrigger("Land");
            }
        }

        // PULO
        if (_jumpPressed && isGrounded && !isLanding)
        {
            _velocity.y = JumpForce;
            _animator.SetBool("isJumping", true);
            isJumping = true;
        }

        // Gravidade
        if (isGrounded && _velocity.y < 0)
            _velocity.y = -1f;
        else
            _velocity.y += GravityValue * Runner.DeltaTime;

        // MOVIMENTO
        Vector3 move = Vector3.zero;

        if (!isLanding)
            move = _moveInput.normalized * PlayerSpeed * Runner.DeltaTime;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        // Animações de movimento
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
}