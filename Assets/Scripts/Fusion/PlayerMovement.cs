using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    public Vector3 _velocity;

    private bool _jumpPressed;
    private Vector3 _moveInput; // Captura input de movimento aqui

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Captura input apenas do jogador local
    void Update()
    {
        // Corrigido: era Object.HasInputAuthority
        if (!HasInputAuthority)
            return;

        // Captura input de movimento e jump
        _moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        // Corrigido: era Object.HasInputAuthority
        if (!HasInputAuthority)
            return;

        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -1f;

        // Usa o input capturado no Update()
        Vector3 move = _moveInput * PlayerSpeed * Runner.DeltaTime;

        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
            _velocity.y = JumpForce;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move.sqrMagnitude > 0.001f)
            transform.forward = move.normalized;

        _jumpPressed = false;
    }
}
