using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementOtavio : NetworkBehaviour
{
    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _jumpPressed;

    [Header("Velocidade")]
    public float PlayerSpeed = 5f;                  // velocidade normal
    public float InteractSpeedMultiplier = 0.5f;    // 50% da velocidade ao interagir

    [Header("Configurações")]
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    [HideInInspector] public bool CanRotate = true;       // controla se o player pode rotacionar
    [HideInInspector] public bool IsInteracting = false;  // indica se o player está empurrando/puxando

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -1f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        // 🔒 Limita o movimento quando estiver empurrando/puxando
        float speed = PlayerSpeed;

        if (IsInteracting)
        {
            // trava eixo
            if (Mathf.Abs(h) > Mathf.Abs(v))
                move = new Vector3(h, 0, 0);
            else
                move = new Vector3(0, 0, v);

            // reduz velocidade
            speed *= InteractSpeedMultiplier;
        }

        move *= speed * Runner.DeltaTime;

        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
            _velocity.y = JumpForce;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        // ✅ Só rotaciona se permitido
        if (move.sqrMagnitude > 0.001f && CanRotate)
            transform.forward = move.normalized;

        _jumpPressed = false;
    }
}



