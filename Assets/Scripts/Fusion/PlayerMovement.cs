using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    private Animator _animator;

    public Vector3 _velocity;
    private Vector3 _moveInput;

    private bool _jumpPressed;
    private bool wasGroundedLastFrame;
    private bool isJumping;

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogWarning("Nenhum Animator encontrado no PlayerMovement.");
    }

    void Update()
    {
        if (!HasInputAuthority)
            return;

        // Captura o input de movimento
        _moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Captura o input de pulo
        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
            return;

        bool isGrounded = _controller.isGrounded;

        // Detecta "Landing" — acabou de tocar o chão
        if (isGrounded && !wasGroundedLastFrame)
        {
            if (_animator)
            {
                _animator.SetTrigger("Land");
                _animator.SetBool("isFalling", false);
                _animator.SetBool("isJumping", false);
            }
            isJumping = false;
        }

        // Corrige velocidade vertical ao tocar o chão
        if (isGrounded && _velocity.y < 0)
            _velocity.y = -1f;

        // Movimento horizontal
        Vector3 move = _moveInput * PlayerSpeed * Runner.DeltaTime;

        // Gravidade
        _velocity.y += GravityValue * Runner.DeltaTime;

        // Pulo
        if (_jumpPressed && isGrounded)
        {
            _velocity.y = JumpForce;
            isJumping = true;

            if (_animator)
            {
                _animator.SetBool("isJumping", true);
                _animator.ResetTrigger("Land");
            }
        }

        // Move o player
        _controller.Move(move + _velocity * Runner.DeltaTime);

        // Atualiza rotação
        if (move.sqrMagnitude > 0.001f)
            transform.forward = move.normalized;

        // Atualiza animações
        UpdateAnimatorStates(isGrounded, move);

        wasGroundedLastFrame = isGrounded;
        _jumpPressed = false;
    }

    private void UpdateAnimatorStates(bool isGrounded, Vector3 move)
    {
        if (_animator == null)
            return;

        float speed = move.magnitude / (PlayerSpeed * Runner.DeltaTime);

        // Define se está parado ou andando
        bool isMoving = speed > 0.05f && isGrounded;
        _animator.SetBool("isIdle", !isMoving);
        _animator.SetBool("isWalking", isMoving);
        _animator.SetFloat("Speed", speed);

        // Controle de queda
        if (!isGrounded && _velocity.y < -1f && isJumping)
        {
            _animator.SetBool("isFalling", true);
        }
        else if (isGrounded)
        {
            _animator.SetBool("isFalling", false);
        }
    }
}
