using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO remove? cuz prob useless state
public class SpawnState : State
{
    public SpawnState(EnemyStateController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
