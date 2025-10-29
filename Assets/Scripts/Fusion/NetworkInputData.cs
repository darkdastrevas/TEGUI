using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons buttons;
    public Vector2 moveDirection;
}

public enum InputButtons
{
    Jump = 1 << 0,
    Climb = 1 << 1
}