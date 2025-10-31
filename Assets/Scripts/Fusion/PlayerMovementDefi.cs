using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementDefi : NetworkBehaviour
{
    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _jumpPressed;

    [Header("Configurações")]
    public float PlayerSpeed = 5f;
    public float JumpForce = 10f;
    public float GravityValue = -9.81f;

    [HideInInspector] public bool CanRotate = true; // <-- nova flag

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

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move *= PlayerSpeed * Runner.DeltaTime;

        _velocity.y += GravityValue * Runner.DeltaTime;

        if (_jumpPressed && _controller.isGrounded)
            _velocity.y = JumpForce;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        // ✅ Só rotaciona se CanRotate for true
        if (move.sqrMagnitude > 0.001f && CanRotate)
            transform.forward = move.normalized;

        _jumpPressed = false;
    }
}


