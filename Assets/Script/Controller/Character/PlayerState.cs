using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    public abstract void OnEnter(PlayerController player);
    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit();
}
