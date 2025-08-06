using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

public class PlayerHangOnState : PlayerState
{
    public override void OnEnter(PlayerController player)
    {
        base.OnEnter(player);
        player.SetRigidActive(false);
        player.SetColliderActive(false);
    }

    public override void LogicUpdate()
    {
        _player.transform.rotation = Quaternion.Euler(0, 0, 0);
        if (Input.GetKey(KeyCode.Space))
        {
            _player.Swing.Release(_player).Forget();
        }
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit(PlayerController player)
    {
        player.SetRigidActive(true);
        player.SetColliderActive(true);
        player.Swing = null;
    }
}