using UnityEngine;
using System.Collections;

public class PlayerHangOnState : PlayerState
{
    public override void OnEnter(PlayerController _player)
    {
        player = _player;
        player.SetRigidActive(false); 
    }

    public override void LogicUpdate()
    {
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit(PlayerController player)
    {
        player.SetRigidActive(true);
    }
}