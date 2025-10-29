using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    public Vector3 _velocity;

    private bool _jumpPressed;

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
        // Isso garante que apenas o jogador dono do objeto capture input
        if (!Object.HasInputAuthority)
            return;

        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    

    public override void FixedUpdateNetwork()
    {
        // Apenas o jogador com autoridade de input deve executar lógica de movimento
        if (!Object.HasInputAuthority)
            return;

        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -1f;

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move *= PlayerSpeed * Runner.DeltaTime;


        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
            _velocity.y = JumpForce;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move.sqrMagnitude > 0.001f)
            transform.forward = move.normalized;

        _jumpPressed = false;

    }

}
