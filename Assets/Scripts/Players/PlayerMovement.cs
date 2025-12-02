using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour, IPlayerMovement
{
    private CharacterController _controller;
    private Animator _animator;
    private LedgeGrab ledgeGrab;

    // Propriedade pública para expor se o player está grounded (implementa IPlayerMovement)
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

    [Header("Lata de Tinta")]
    public GameObject paintCan;
    public float paintCanDuration = 2f;
    private bool paintInUse = false;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        ledgeGrab = GetComponent<LedgeGrab>();

        if (paintCan != null)
            paintCan.SetActive(false); // começa invisível

        // Encontra e configura a HUD existente na cena
        if (Object.HasInputAuthority)
        {
            var hudObject = GameObject.Find("PlayerHUD"); // Encontra o objeto HUD na cena
            if (hudObject != null)
            {
                var hudScript = hudObject.GetComponent<PlayerHUD>();
                if (hudScript != null)
                {
                    hudScript.SetPlayer(this); // Configura o ícone baseado na tag do jogador
                    Debug.Log("[PlayerMovement] HUD da cena configurada para jogador local.");
                }
                else
                {
                    Debug.LogWarning("[PlayerMovement] PlayerHUD script não encontrado no objeto da cena.");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerMovement] PlayerHUD objeto não encontrado na cena.");
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

        // BLOQUEIO TOTAL DURANTE A LATA
        if (paintInUse)
        {
            // Não faz nada aqui, pois o movimento é bloqueado na coroutine
            return;
        }

        // BLOQUEIO TOTAL DURANTE ESCALADA
        if (ledgeGrab != null && (ledgeGrab.isGrabbing || ledgeGrab.isClimbing))
        {
            _velocity.y = -1f;
            _jumpPressed = false;
            return;
        }

        if (IsMovementBlocked)
        {
            // Se travado, garantimos que o player pare completamente
            _velocity = Vector3.zero;
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

    // --------------------------
    //     SISTEMA DA LATA
    // --------------------------

    private System.Collections.IEnumerator UsePaintCanRoutine()
    {
        // Verificação reforçada: só ativa se estiver no chão
        if (!_controller.isGrounded)
        {
            Debug.Log("[PlayerMovement] Spray can só pode ser usado no chão!");
            yield break; // Sai da coroutine sem ativar
        }

        paintInUse = true;

        // ativa a lata
        if (paintCan != null)
            paintCan.SetActive(true);

        // espera o tempo configurado
        yield return new WaitForSeconds(paintCanDuration);

        // desativa a lata
        if (paintCan != null)
            paintCan.SetActive(false);

        // libera movimento
        SetMovementBlocked(false);

        paintInUse = false;
    }

    private void TryUsePaintCan()
    {
        // Verificação inicial: só pode tentar usar no chão
        if (!_controller.isGrounded)
            return;

        // evitar spam
        if (paintInUse)
            return;

        Runner.StartCoroutine(UsePaintCanRoutine());
    }

    // Implementação da interface: Bloqueia/desbloqueia movimento
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
