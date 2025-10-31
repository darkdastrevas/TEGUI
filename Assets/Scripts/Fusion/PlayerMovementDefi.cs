using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementDefi : NetworkBehaviour
{
    private CharacterController _controller;
    private Animator _animator; // Referência ao Animator
    public Vector3 _velocity;

    private bool _jumpPressed;
    private Vector3 _moveInput; // Captura input de movimento aqui
    private bool isJumping; // Flag pra animação de pulo

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>(); // Pega o Animator (assumindo que tá no filho, como modelo)
    }

    // Captura input apenas do jogador local
    void Update()
    {
        // Corrigido: era Object.HasInputAuthority
        if (!HasInputAuthority)
            return;

        // Captura input de movimento e jump
        _moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Jump") && !isJumping) // Só permite jump se não estiver pulando
        {
            _jumpPressed = true;
        }

        // Verifica se a animação de jump terminou
        if (isJumping && _animator != null)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Jump") && stateInfo.normalizedTime >= 1.0f) // Animação terminou
            {
                isJumping = false;
                _animator.SetBool("isJumping", false); // Reseta bool se usado
                Debug.Log("Animação de pulo terminou - movimento liberado");
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Corrigido: era Object.HasInputAuthority
        if (!HasInputAuthority)
            return;

        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -1f;

        // Se estiver pulando (animação), não move fisicamente
        if (isJumping)
        {
            // Trava movimento e rotação durante a animação
            _controller.Move(Vector3.zero); // Sem movimento
            return; // Sai cedo, sem aplicar física
        }

        // Usa o input capturado no Update()
        Vector3 move = _moveInput * PlayerSpeed * Runner.DeltaTime;

        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
        {
            // Inicia animação de pulo (sem pular fisicamente)
            isJumping = true;
            if (_animator != null)
            {
                _animator.SetBool("isJumping", true); // Ou _animator.SetTrigger("Jump") se usar trigger
                Debug.Log("Iniciando animação de pulo");
            }
            _jumpPressed = false; // Reseta
            return; // Não aplica jump físico
        }

        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move.sqrMagnitude > 0.001f)
            transform.forward = move.normalized;

        // Integração com Animator (Idle/Walk)
        if (_animator != null && !isJumping)
        {
            bool isGrounded = _controller.isGrounded;
            float horizontalSpeed = move.magnitude / (PlayerSpeed * Runner.DeltaTime); // Normaliza velocidade (0-1)

            if (horizontalSpeed > 0.1f)
            {
                // Walk: Grounded e movendo
                _animator.SetBool("isWalking", true);
            }
            else
            {
                // Idle: Grounded e parado
                _animator.SetBool("isWalking", false);
            }

            // Opcional: Setar velocidade pra blend trees
            _animator.SetFloat("Speed", horizontalSpeed);
        }

        _jumpPressed = false;
    }
}

