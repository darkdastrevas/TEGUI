using UnityEngine;

public interface IPlayerMovement
{
    bool IsGrounded { get; }
    void SetMovementBlocked(bool isBlocked);
}